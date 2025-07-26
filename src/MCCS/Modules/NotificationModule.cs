
using MCCS.Components.GlobalNotification;
using MCCS.Services.NotificationService;

namespace MCCS.Modules
{
    public sealed class NotificationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {}

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
            containerRegistry.Register<NotificationContainer>();
        }
    }
}
