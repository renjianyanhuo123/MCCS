using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

using MaterialDesignThemes.Wpf;

namespace MCCS.Common.Resources.Extensions
{
    public class MaterialDialogService : DialogService
    {
        private readonly IContainerExtension _containerExtension;

        public MaterialDialogService(IContainerExtension containerExtension) : base(containerExtension)
        {
            _containerExtension = containerExtension;
        }

        public void ShowDialogHost(string name, IDialogParameters parameters, Action<IDialogResult> callback) =>
            ShowDialogHost(name, null, parameters, callback);

        public void ShowDialogHost(string name, string dialogHostName, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            parameters ??= new DialogParameters();

            var content = _containerExtension.Resolve<object>(name);
            if (content is not FrameworkElement dialogContent)
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");

            AutowireViewModel(dialogContent);

            if (dialogContent.DataContext is not IDialogAware dialogAware)
                throw new ArgumentException("A dialog's ViewModel must implement IDialogAware interface");

            // 记录结果，确保 callback 只触发一次
            IDialogResult resultFromVm = null;
            var callbackInvoked = false;

            // ✅ Prism 9：由 DialogService 初始化只读 RequestClose（DialogCloseListener）
            // 让 VM 里调用 RequestClose.Invoke(...) 时能走到这里
            DialogUtilities.InitializeListener(dialogAware, Complete); // :contentReference[oaicite:2]{index=2}

            var openedEventHandler = new DialogOpenedEventHandler((sender, args) =>
            {
                dialogAware.OnDialogOpened(parameters);
            });

            // ✅ 支持 CanCloseDialog：不允许关闭就取消本次 close（例如点遮罩、ESC、CloseDialogCommand）
            var closingEventHandler = new DialogClosingEventHandler((sender, args) =>
            {
                if (!dialogAware.CanCloseDialog())
                    args.Cancel(); // MaterialDesign 的取消方式 :contentReference[oaicite:3]{index=3}
            });

            var closedEventHandler = new DialogClosedEventHandler((sender, args) =>
            {
                dialogAware.OnDialogClosed();

                // 如果是“点遮罩/ESC”等路径关闭，没有走 RequestClose.Invoke，则给一个默认结果
                if (!callbackInvoked)
                {
                    var fallback = resultFromVm ?? new DialogResult(ButtonResult.None);
                    callbackInvoked = true;
                    callback?.Invoke(fallback);
                }
            });

            var dispatcherFrame = new DispatcherFrame();

            Task showTask = dialogHostName == null
                ? DialogHost.Show(dialogContent, openedEventHandler, closingEventHandler, closedEventHandler)
                : DialogHost.Show(dialogContent, dialogHostName, openedEventHandler, closingEventHandler, closedEventHandler);

            showTask.ContinueWith(_ => dispatcherFrame.Continue = false,
                TaskScheduler.FromCurrentSynchronizationContext());

            try
            {
                ComponentDispatcher.PushModal();
                Dispatcher.PushFrame(dispatcherFrame);
            }
            finally
            {
                ComponentDispatcher.PopModal();
            }

            return;

            void Complete(IDialogResult r)
            {
                // 防止重复关闭/重复回调
                if (callbackInvoked) return;
                callbackInvoked = true;

                resultFromVm = r ?? new DialogResult(ButtonResult.None);

                // 确保在 UI 线程关闭 DialogHost
                if (Application.Current?.Dispatcher?.CheckAccess() == false)
                {
                    Application.Current.Dispatcher.Invoke(() => Complete(resultFromVm));
                    return;
                }

                callback?.Invoke(resultFromVm);

                if (DialogHost.IsDialogOpen(dialogHostName))
                    DialogHost.Close(dialogHostName);
            }
        }

        public Task<IDialogResult> ShowDialogHostAsync(
            string name,
            IDialogParameters? parameters = null,
            CancellationToken cancellationToken = default)
            => ShowDialogHostAsync(name, dialogHostName: null, parameters, cancellationToken);

        public async Task<IDialogResult> ShowDialogHostAsync(
            string name,
            string? dialogHostName,
            IDialogParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new DialogParameters();

            // 确保在 UI 线程执行（DialogHost 需要在 UI 线程）
            if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
            {
                return await dispatcher.InvokeAsync(
                    () => ShowDialogHostAsync(name, dialogHostName, parameters, cancellationToken)
                ).Task.Unwrap();
            }

            var content = _containerExtension.Resolve<object>(name);
            if (content is not FrameworkElement dialogContent)
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");

            AutowireViewModel(dialogContent);

            if (dialogContent.DataContext is not IDialogAware dialogAware)
                throw new ArgumentException("A dialog's ViewModel must implement IDialogAware interface");

            // 关键：用 TCS 把 callback 模式变成 await
            var tcs = new TaskCompletionSource<IDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 记录：是否已通过 RequestClose 完成
            var completedByVm = 0;

            void TryComplete(IDialogResult? r)
            {
                if (Interlocked.Exchange(ref completedByVm, 1) == 1) return;
                tcs.TrySetResult(r ?? new DialogResult(ButtonResult.None));
            }

            // Prism 9+：初始化只读 RequestClose
            // DialogUtilities 有 Action/Func<Task> 两种重载，这里用 Func<Task>，便于在关闭前做异步清理 :contentReference[oaicite:2]{index=2}
            DialogUtilities.InitializeListener(dialogAware, async r =>
            {
                TryComplete(r);

                // Close 要在 UI 线程
                if (DialogHost.IsDialogOpen(dialogHostName))
                    DialogHost.Close(dialogHostName);

                await Task.CompletedTask;
            });

            // MaterialDesign: Show(...) 支持 opened/closing/closed handler，closing 可以 Cancel :contentReference[oaicite:3]{index=3}
            var openedHandler = new DialogOpenedEventHandler((_, __) =>
            {
                dialogAware.OnDialogOpened(parameters);
            });

            var closingHandler = new DialogClosingEventHandler((_, args) =>
            {
                // Prism 语义：不允许关闭就拦截（例如点遮罩/ESC/CloseDialogCommand）
                if (!dialogAware.CanCloseDialog())
                    args.Cancel();
            });

            var closedHandler = new DialogClosedEventHandler((_, __) =>
            {
                dialogAware.OnDialogClosed();

                // 如果用户用“点遮罩/ESC”等方式关闭且 VM 没 RequestClose，则给一个兜底结果
                if (tcs.Task.Status is TaskStatus.WaitingForActivation or TaskStatus.WaitingForChildrenToComplete or TaskStatus.WaitingToRun or TaskStatus.Running)
                {
                    // 上面状态判断不够可靠，简单点：只要没 TrySetResult，就用 None 结束
                }

                if (Interlocked.CompareExchange(ref completedByVm, 1, 1) == 0)
                    TryComplete(new DialogResult(ButtonResult.None));
            });

            // 支持取消：取消时尝试关闭 Dialog，并让 Task 变成 canceled（你也可以改成返回 Cancel）
            await using var ctr = cancellationToken.Register(() =>
            {
                if (DialogHost.IsDialogOpen(dialogHostName))
                    DialogHost.Close(dialogHostName);

                tcs.TrySetCanceled(cancellationToken);
            });

            // 直接 await DialogHost.Show：不阻塞 UI 线程
            _ = dialogHostName is null
                ? DialogHost.Show(dialogContent, openedHandler, closingHandler, closedHandler)
                : DialogHost.Show(dialogContent, dialogHostName, openedHandler, closingHandler, closedHandler);

            return await tcs.Task;
        }
        private static void AutowireViewModel(object viewOrViewModel)
        {
            if (viewOrViewModel is FrameworkElement { DataContext: null } view
                && ViewModelLocator.GetAutoWireViewModel(view) is null)
            {
                ViewModelLocator.SetAutoWireViewModel(view, true);
            }
        }
    }
}
