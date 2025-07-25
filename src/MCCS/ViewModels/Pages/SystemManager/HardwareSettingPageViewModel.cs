using System.Collections.ObjectModel;
using System.Windows;
using MCCS.Core.Repositories;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class HardwareSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "HardwareSetting";

        private readonly IChannelAggregateRepository _channelAggregateRepository;
        private readonly IRegionManager _regionManager;

        private ObservableCollection<ChannelVariableInfoViewModel> _channelVariableInfo;
          
        public HardwareSettingPageViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IChannelAggregateRepository channelAggregateRepository,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _regionManager = regionManager;
            _channelVariableInfo = [];
            _channelAggregateRepository = channelAggregateRepository;
        }

        #region Command 
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand<RoutedPropertyChangedEventArgs<object>> SelectedCommand => new(ExecuteSelectedCommand);
        public DelegateCommand AddChannelCommand => new(ExecuteAddChannelCommand);
        public DelegateCommand AddVariableCommand => new(ExecuteAddVariableCommand);
        #endregion

        #region Property
        public ObservableCollection<ChannelVariableInfoViewModel> ChannelVariableInfo
        {
            get => _channelVariableInfo;
            set => SetProperty(ref _channelVariableInfo, value);
        }
        #endregion

        #region private method

        private void ExecuteAddVariableCommand()
        {
            // _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(AddChannelPageViewModel.Tag, UriKind.Relative));
        }

        private void ExecuteAddChannelCommand()
        {
            _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(AddChannelPageViewModel.Tag, UriKind.Relative));
        }

        private void ExecuteSelectedCommand(RoutedPropertyChangedEventArgs<object> param)
        {
            var parameters = new NavigationParameters(); 
            switch (param.NewValue)
            {
                case ChannelVariableInfoViewModel channel: 
                    parameters.Add("ChannelId", channel.ChannelId);
                    _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(ChannelSettingPageViewModel.Tag, UriKind.Relative), parameters);
                    break;
                case VariableInfoViewModel variable:
                    parameters.Add("VariableId", variable.VariableId);
                    _regionManager.RequestNavigate(GlobalConstant.SystemManagerHardwarePageRegionName, new Uri(VariableSettingPageViewModel.Tag, UriKind.Relative), parameters);
                    break;
                default:
                    // Debug.WriteLine("Unknown type selected");
                    break;
            }

            // Debug.WriteLine(param);
        }

        private async Task ExecuteLoadCommand()
        {
            _channelVariableInfo.Clear();
            var channelInfos = await _channelAggregateRepository.GetChannelsAsync();
            foreach (var channelInfo in channelInfos)
            {
                _channelVariableInfo.Add(new ChannelVariableInfoViewModel()
                {
                    ChannelId = channelInfo.ChannelInfo.Id,
                    ChannelName = channelInfo.ChannelInfo.ChannelName,
                    VariableInfos = channelInfo.Variables
                        .Select(variable => new VariableInfoViewModel()
                        {
                            VariableId = variable.Id,
                            VariableName = variable.Name
                        }).ToList()
                });
            }
        }

        #endregion
    }
}
