using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.ViewModels.Others.SystemManager;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public class HardwareSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "HardwareSetting";

        private readonly IChannelAggregateRepository _channelAggregateRepository;

        private ObservableCollection<ChannelVariableInfoViewModel> _channelVariableInfo;
          
        public HardwareSettingPageViewModel(
            IEventAggregator eventAggregator,
            IChannelAggregateRepository channelAggregateRepository,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _channelVariableInfo = [];
            _channelAggregateRepository = channelAggregateRepository;
        }

        #region Command

        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        #endregion

        #region Property
        public ObservableCollection<ChannelVariableInfoViewModel> ChannelVariableInfo
        {
            get => _channelVariableInfo;
            set => SetProperty(ref _channelVariableInfo, value);
        }
        #endregion

        #region private method

        private async Task ExecuteLoadCommand()
        {
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
