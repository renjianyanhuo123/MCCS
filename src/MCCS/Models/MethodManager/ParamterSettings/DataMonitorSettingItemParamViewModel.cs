using MCCS.Models.CurveModels;

namespace MCCS.Models.MethodManager.ParamterSettings
{
    public class DataMonitorSettingItemParamViewModel : BindableBase
    {
        private XyBindCollectionItem? _selectedChannelItem; 
        public XyBindCollectionItem? SelectedChannelItem
        {
            get => _selectedChannelItem; 
            set => SetProperty(ref _selectedChannelItem, value);
        }

        /// <summary>
        /// 专门用于显示的单位(int类型写死于界面中)
        /// </summary>
        private int _unit;
        public int Unit { get => _unit; set => SetProperty(ref _unit, value); }

        private int _retainBit;
        public int RetainBit { get => _retainBit; set => SetProperty(ref _retainBit, value); }
    }
}
