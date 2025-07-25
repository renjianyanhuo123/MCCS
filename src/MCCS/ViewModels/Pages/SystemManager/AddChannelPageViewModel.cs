
using MaterialDesignThemes.Wpf;
using MCCS.Core.Helper;
using MCCS.Core.Models.SystemManager;
using MCCS.Core.Repositories;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class AddChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "AddChannel";

        private readonly IChannelAggregateRepository _channelAggregateRepository;
        

        public AddChannelPageViewModel(
            IEventAggregator eventAggregator, 
            IDialogService? dialogService, 
            IChannelAggregateRepository channelAggregateRepository) : base(eventAggregator, dialogService)
        {
            _channelAggregateRepository = channelAggregateRepository;
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

        private ISnackbarMessageQueue _snackbarMessageQueue;

        public ISnackbarMessageQueue MessageQueue
        {
            get => _snackbarMessageQueue;
            set => SetProperty(ref _snackbarMessageQueue, value);
        }

        #endregion

        #region Command

        public AsyncDelegateCommand AddChannelCommand => new(ExexuteAddChannelCommand);

        #endregion

        #region private method

        private async Task ExexuteAddChannelCommand()
        {
            var channelInfo = new ChannelInfo
            {
                ChannelId = $"Channel_{HighPerformanceRandomHash.GenerateRandomHash6()}",
                ChannelName = ChannelName,
                IsShowable = IsShowable,
                IsOpenSpecimenProtected = IsOpenProtected
            };
            await _channelAggregateRepository.AddChannelAsync(channelInfo);
            MessageQueue?.Enqueue(
                $"保存成功！",
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(2));
        }

        #endregion
    }
}
