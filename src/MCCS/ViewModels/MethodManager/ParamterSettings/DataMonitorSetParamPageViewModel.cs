using System.Collections.ObjectModel;

using MCCS.Collecter.PseudoChannelManagers;
using MCCS.Models.CurveModels;
using MCCS.Models.MethodManager.ParamterSettings;

using Newtonsoft.Json;

namespace MCCS.ViewModels.MethodManager.ParamterSettings
{
    public class DataMonitorSetParamPageViewModel : BaseParameterSetViewModel<List<DataMonitorSettingItemParamModel>>
    {
        private readonly IPseudoChannelManager _pseudoChannelManager;

        public DataMonitorSetParamPageViewModel(IPseudoChannelManager pseudoChannelManager, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _pseudoChannelManager = pseudoChannelManager;
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

        protected override void ExecuteLoad()
        {
            SettingValues.Clear();
            PseudoChannels.Clear();
            var channels = _pseudoChannelManager.GetPseudoChannels();
            foreach (var channel in channels)
            {
                var tempModel = new XyBindCollectionItem
                {
                    Id = channel.ChannelId,
                    Name = channel.Configuration.ChannelName,
                    Unit = channel.Configuration.Unit ?? "",
                    DisplayName = channel.Configuration.ChannelName
                };
                PseudoChannels.Add(tempModel); 
            } 
            if (Parameter == null) return;
            foreach (var item in Parameter)
            {
                SettingValues.Add(new DataMonitorSettingItemParamViewModel
                {
                    SelectedChannelItem = PseudoChannels.FirstOrDefault(c => c.Id == item.PseudoChannelId),
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
                PseudoChannelId = s.SelectedChannelItem?.Id ?? 0,
                Unit = s.Unit,
                RetainBit = s.RetainBit
            }).ToList();
            var json = JsonConvert.SerializeObject(settingValues);
            return json;
        }
    }
}
