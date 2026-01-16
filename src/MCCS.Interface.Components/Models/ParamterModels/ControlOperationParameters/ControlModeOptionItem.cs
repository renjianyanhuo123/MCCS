using MCCS.Interface.Components.Enums;

using Prism.Mvvm;

namespace MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters
{
    /// <summary>
    /// 控制模式选项项（支持多选）
    /// </summary>
    public class ControlModeOptionItem : BindableBase
    {
        /// <summary>
        /// 控制模式类型
        /// </summary>
        public ControlModeTypeEnum Mode { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        private bool _isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
