using System.Collections.ObjectModel;
using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BindableBase
    {
        public const string Tag = "StaticControl";

        public ViewStaticControlViewModel(IEnumerable<ControlChannelBindModel> channels)
        {
            foreach (var channel in channels)
            {
                Channels.Add(channel);
            }
        }

        #region 页面属性

        public ObservableCollection<ControlChannelBindModel> Channels { get; private set; } = [];

        private ControlChannelBindModel _selectedChannel;
        public ControlChannelBindModel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (SetProperty(ref _selectedChannel, value))
                {
                    SelectedControlUnitType = (int)_selectedChannel.ChannelType;
                }
            }
        }

        /// <summary>
        /// 0-位移
        /// 1-力 
        /// </summary>
        public int SelectedControlUnitType { get; set; }

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
        private float _targetValue = 0.0f;
        public float TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }
        #endregion 
    }
}
