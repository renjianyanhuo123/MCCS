using System.Collections.ObjectModel;

using MCCS.Infrastructure.Repositories;
using MCCS.Interface.Components.Models;

using Newtonsoft.Json;

namespace MCCS.Interface.Components.ViewModels
{
    public class DataMonitorSetParamPageViewModel : BaseParameterSetViewModel<List<DataMonitorSettingItemParamModel>>
    {
        private readonly IStationSiteAggregateRepository _siteAggregateRepository;

        public DataMonitorSetParamPageViewModel(IStationSiteAggregateRepository siteAggregateRepository, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _siteAggregateRepository = siteAggregateRepository;
            AddDataSettingCommand = new DelegateCommand(ExecuteAddDataSetting);
            DeleteSettingItemCommand = new DelegateCommand<DataMonitorSettingItemParamViewModel>(ExecuteDeleteSettingItemCommand); 
        }

        #region Command 
        public DelegateCommand AddDataSettingCommand { get; }
        public DelegateCommand<DataMonitorSettingItemParamViewModel> DeleteSettingItemCommand { get; } 
        #endregion

        #region Property
        public ObservableCollection<XyBindCollectionItem> PseudoChannels { get; } = [];

        public ObservableCollection<DataMonitorSettingItemParamViewModel> SettingValues { get; } = [];
        #endregion

        #region Private Method 
        private void ExecuteAddDataSetting() => SettingValues.Add(new DataMonitorSettingItemParamViewModel());

        private void ExecuteDeleteSettingItemCommand(DataMonitorSettingItemParamViewModel param) => SettingValues.Remove(param);

        protected override async Task ExecuteLoad()
        {
            SettingValues.Clear();
            PseudoChannels.Clear();
            var stationSiteAggregate = await _siteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            foreach (var channel in stationSiteAggregate.PseudoChannelInfos)
            {
                var tempModel = new XyBindCollectionItem
                {
                    Id = channel.PseudoChannelInfo.Id,
                    Name = channel.PseudoChannelInfo.ChannelName,
                    Unit = channel.PseudoChannelInfo.Unit ?? "",
                    DisplayName = channel.PseudoChannelInfo.ChannelName
                };
                PseudoChannels.Add(tempModel); 
            } 
            if (Parameter == null) return;
            foreach (var item in Parameter)
            {
                SettingValues.Add(new DataMonitorSettingItemParamViewModel
                {
                    SelectedChannelItem = PseudoChannels.FirstOrDefault(c => c.Id == item.PseudoChannel.Id),
                    RetainBit = item.RetainBit,
                    Unit = item.Unit
                }); 
            }
        }
        #endregion

        protected override string GetParameterJson()
        {
            var settingValues = SettingValues.Select(s => new DataMonitorSettingItemParamModel
            {
                PseudoChannel = s.SelectedChannelItem,
                Unit = s.Unit,
                RetainBit = s.RetainBit
            }).ToList();
            var json = JsonConvert.SerializeObject(settingValues);
            return json;
        }
    }
}
