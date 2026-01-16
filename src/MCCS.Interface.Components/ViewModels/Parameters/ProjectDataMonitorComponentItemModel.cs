using System.Runtime.CompilerServices;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    /// <summary>
    /// 数据监控组件项模型
    /// 针对高频更新(100ms)进行了性能优化
    /// </summary>
    public sealed class ProjectDataMonitorComponentItemModel : BindableBase
    {
        /// <summary>
        /// 用于存储对外获取数据的ID (虚拟通道ID)
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// 展示的中文名称
        /// </summary>
        private string _displayName = "";
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        /// <summary>
        /// 原始数值（内部使用，不触发UI更新）
        /// </summary>
        private double _value;

        /// <summary>
        /// 显示值 - 内部更新时不触发PropertyChanged，由批量刷新触发
        /// </summary>
        public double Value
        {
            get => _value;
            set => _value = value; // 直接赋值，不触发通知
        }

        /// <summary>
        /// 格式化后的显示值（用于UI绑定）
        /// </summary>
        private string _formattedValue = "0";
        public string FormattedValue
        {
            get => _formattedValue;
            private set => SetProperty(ref _formattedValue, value);
        }

        /// <summary>
        /// 专门用于显示的单位
        /// </summary>
        private string _unit = "";
        public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

        /// <summary>
        /// 保留小数点后位数
        /// </summary>
        private int _retainBit;
        public int RetainBit
        {
            get => _retainBit;
            set
            {
                if (SetProperty(ref _retainBit, value))
                {
                    UpdateFormatString();
                }
            }
        }

        /// <summary>
        /// 缓存的格式化字符串，避免每次更新时创建
        /// </summary>
        private string _formatString = "F2";

        /// <summary>
        /// 更新格式化字符串缓存
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateFormatString()
        {
            _formatString = $"F{_retainBit}";
        }

        /// <summary>
        /// 批量刷新UI（由ViewModel在定时器中调用）
        /// 仅当值发生变化时才更新FormattedValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RefreshDisplay()
        {
            var newFormatted = _value.ToString(_formatString);
            if (!string.Equals(_formattedValue, newFormatted, StringComparison.Ordinal))
            {
                FormattedValue = newFormatted;
            }
        }

        /// <summary>
        /// 直接更新值（高性能方法，不触发任何通知）
        /// </summary>
        /// <param name="newValue">新值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValueDirect(double newValue)
        {
            _value = newValue;
        }
    }
}
