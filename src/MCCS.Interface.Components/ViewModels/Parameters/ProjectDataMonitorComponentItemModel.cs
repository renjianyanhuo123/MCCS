using MCCS.Infrastructure.Services;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    public class ProjectDataMonitorComponentItemModel : BindableBase
    { 
        /// <summary>
        /// 用于存储对外获取数据的ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 展示的中文名称
        /// </summary>
        private string _displayName = "";
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        /// <summary>
        /// 显示值
        /// </summary>  
        private double _value; 
        public double Value
        {
            get => _value; 
            set => SetProperty(ref _value, value);
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
        public int RetainBit { get => _retainBit; set => SetProperty(ref _retainBit, value); }
    }
}
