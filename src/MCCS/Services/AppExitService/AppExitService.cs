using System.Windows;

using MCCS.Events.AppExit;

namespace MCCS.Services.AppExitService
{
    public class AppExitService(IEventAggregator eventAggregator) : IAppExitService
    {
        public void Exit()
        { 
            eventAggregator.GetEvent<AppExitingEvent>().Publish(new AppExitingEventParam()); 
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        public async Task ExitAsync()
        {
            // 1. 广播退出事件
            eventAggregator.GetEvent<AppExitingEvent>().Publish(new AppExitingEventParam());

            // 2. 关闭主窗口
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Application.Current.Shutdown();
            });
        }
    }
}
