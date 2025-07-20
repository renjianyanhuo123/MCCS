using System.Collections.ObjectModel;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class ChannelSettingPageViewModel:BaseViewModel
    {
        public const string Tag = "ChannelSetting";

        public ChannelSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public ChannelSettingPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }

        #region Property

        public ObservableCollection<ChannelHardwareViewModel> ChannelHardwareInfo { get; private set; } = [];

        #endregion

        #region Command
        public AsyncDelegateCommand LoadedCommand => new(ExecuteLoadedCommand);
        #endregion

        #region private method

        private async Task ExecuteLoadedCommand()
        {

        }

        #endregion
    }
}
