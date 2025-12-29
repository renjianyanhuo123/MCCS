namespace MCCS.Extensions
{
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Shows a modal dialog using a <see cref="MaterialDesignThemes.Wpf.DialogHost"/>.
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <exception cref="NullReferenceException">Thrown when the dialog service is not a MaterialDialogService</exception>
        public static void ShowDialogHost(this IDialogService dialogService, string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            if (dialogService is not MaterialDialogService materialDialogService)
                throw new NullReferenceException("DialogService must be a MaterialDialogService");

            materialDialogService.ShowDialogHost(name, parameters, callback);
        }

        /// <summary>
        /// Shows a modal dialog using a <see cref="MaterialDesignThemes.Wpf.DialogHost"/>.
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <param name="windowName">The name of the <see cref="MaterialDesignThemes.Wpf.DialogHost"/> that will contain the dialog control</param>
        /// <exception cref="NullReferenceException">Thrown when the dialog service is not a MaterialDialogService</exception>
        public static void ShowDialogHost(this IDialogService dialogService, string name,
            IDialogParameters parameters, Action<IDialogResult> callback, string windowName)
        {
            if (dialogService is not MaterialDialogService materialDialogService)
                throw new NullReferenceException("DialogService must be a MaterialDialogService");

            materialDialogService.ShowDialogHost(name, windowName, parameters, callback);
        }

        public static Task<IDialogResult> ShowDialogHostAsync(this IDialogService dialogService, string name, IDialogParameters parameters, CancellationToken cancellationToken = default)
        {
            if (dialogService is not MaterialDialogService materialDialogService)
                throw new NullReferenceException("DialogService must be a MaterialDialogService");
            return materialDialogService.ShowDialogHostAsync(name, parameters, cancellationToken);
        }

        public static Task<IDialogResult> ShowDialogHostAsync(this IDialogService dialogService, string name, string? dialogHostName,
            IDialogParameters parameters, CancellationToken cancellationToken = default)
        {
            if (dialogService is not MaterialDialogService materialDialogService)
                throw new NullReferenceException("DialogService must be a MaterialDialogService");
            return materialDialogService.ShowDialogHostAsync(name, dialogHostName, parameters, cancellationToken);
        }
    }
}
