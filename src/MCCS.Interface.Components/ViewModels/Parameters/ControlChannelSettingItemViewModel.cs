using System.Collections.ObjectModel;

using MCCS.Infrastructure.Helper;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    /// <summary>
    /// 单个控制通道设置项的 ViewModel
    /// </summary>
    public class ControlChannelSettingItemViewModel : BindableBase
    {  
        private bool _selected;
        /// <summary>
        /// 当前是否选中 
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }
        /// <summary>
        /// 通道ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 控制模式选项列表（支持多选）
        /// </summary>
        public ObservableCollection<ControlModeOptionItem> ControlModeOptions { get; } = [];

        public ControlChannelSettingItemViewModel()
        {
            var controlModeOptions = (from ControlModeTypeEnum status in Enum.GetValues(typeof(ControlModeTypeEnum))
                    select new ControlModeOptionItem { Mode = status, DisplayName = EnumHelper.GetDescription(status), IsSelected = false })
                .ToList();
            ControlModeOptions.AddRange(controlModeOptions); 
        } 

        /// <summary>
        /// 获取选中的控制模式列表
        /// </summary>
        public List<ControlModeTypeEnum> GetSelectedControlModes() =>
            ControlModeOptions
                .Where(o => o.IsSelected)
                .Select(o => o.Mode)
                .ToList();

        /// <summary>
        /// 设置选中的控制模式
        /// </summary>
        public void SetSelectedControlModes(List<ControlModeTypeEnum> modes)
        {
            foreach (var option in ControlModeOptions)
            {
                option.IsSelected = modes.Contains(option.Mode);
            }
        }
    }
}
