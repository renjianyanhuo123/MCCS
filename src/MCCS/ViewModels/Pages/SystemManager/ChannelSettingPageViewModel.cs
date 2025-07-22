using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class ChannelSettingPageViewModel:BaseViewModel
    {
        public const string Tag = "ChannelSetting";

        private readonly IChannelAggregateRepository _channelAggregateRepository;

        public ChannelSettingPageViewModel(
            IChannelAggregateRepository channelAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _channelAggregateRepository = channelAggregateRepository;
        }

        #region Property

        public ObservableCollection<ChannelHardwareViewModel> ChannelHardwareInfo { get; private set; } = [];

        #endregion

        #region Command
        // public AsyncDelegateCommand LoadedCommand => new(ExecuteLoadedCommand);
        #endregion

        #region private method 

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var channelId = navigationContext.Parameters.GetValue<long>("ChannelId");
            var hardwareInfos = _channelAggregateRepository.GetHardwareInfoByChannelId(channelId);
            if (hardwareInfos == null) throw new ArgumentNullException(nameof(hardwareInfos));
            foreach (var hardware in hardwareInfos)
            {
                ChannelHardwareInfo.Add(new ChannelHardwareViewModel
                {
                    ControllerName = hardware.Name,
                    HardwareId = hardware.Id,
                });
            }
        }

        #endregion
    }
}
