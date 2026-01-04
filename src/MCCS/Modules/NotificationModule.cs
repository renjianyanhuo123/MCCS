
using MCCS.Common.Resources.Extensions;
using MCCS.Common.Resources.ViewModels;

namespace MCCS.Modules
{
    public sealed class NotificationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {}

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<INotificationService, NotificationViewModel>(); 
        }
    }
}
