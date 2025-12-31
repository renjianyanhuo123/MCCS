using MCCS.Common.Resources.ViewModels;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Events.SystemManager;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class AddChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "AddChannel";

        private readonly INotificationService _notificationService;

        public AddChannelPageViewModel(
            INotificationService notificationService,
            IEventAggregator eventAggregator, 
            IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
            _notificationService = notificationService;
        }

        #region Property
        private string _channelName = string.Empty;
        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        private bool _isShowable = false;
        public bool IsShowable
        {
            get => _isShowable;
            set => SetProperty(ref _isShowable, value);
        }

        private bool _isOpenProtected = false; 
        public bool IsOpenProtected
        {
            get => _isOpenProtected;
            set => SetProperty(ref _isOpenProtected, value);
        } 
        #endregion

        #region Command 
        public AsyncDelegateCommand AddChannelCommand => new(ExexuteAddChannelCommand); 
        #endregion

        #region private method

        private async Task ExexuteAddChannelCommand()
        {
            if (string.IsNullOrEmpty(ChannelName)) return;
            //var channelInfo = new ChannelInfo
            //{
            //    ChannelId = $"Channel_{HighPerformanceRandomHash.GenerateRandomHash6()}",
            //    ChannelName = ChannelName,
            //    IsShowable = IsShowable,
            //    IsOpenSpecimenProtected = IsOpenProtected
            //};
            //var addedChannelId = await _channelAggregateRepository.AddChannelAsync(channelInfo);
            _notificationService.Show(
                "添加通道成功",
                "通道信息已成功添加到系统中!",
                NotificationType.Success,
                3);
            _eventAggregator.GetEvent<NotificationAddChannelEvent>()
                .Publish(new NotificationAddChannelEventParam
                {
                    ChannelId = 1,
                    ChannelName = ChannelName
                });
        }

        #endregion
    }
}
