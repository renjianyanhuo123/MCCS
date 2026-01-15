using System.Collections.ObjectModel;

using MCCS.Interface.Components.Models;

namespace MCCS.Interface.Components.ViewModels.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseControlViewModel
    {
        public const string Tag = "StaticControl";
         
        #region 页面属性

        public ObservableCollection<ControlChannelBindModel> Channels { get; private set; } = [];

        /// <summary>
        /// 0-位移
        /// 1-力 
        /// </summary>
        private int _selectedControlUnitType;
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType; 
            set =>SetProperty(ref _selectedControlUnitType, value);
        }

        /// <summary>
        /// 0 - mm
        /// 1 - kN
        /// </summary>
        private int _selectedTargetUnitType; 
        public int SelectedTargetUnitType
        {
            get => _selectedTargetUnitType;
            set => SetProperty(ref _selectedTargetUnitType, value);
        }

        private float _speed = 0.0f;
        public float Speed 
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }
        private float _targetValue;
        public float TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }
        #endregion 
    }
}
