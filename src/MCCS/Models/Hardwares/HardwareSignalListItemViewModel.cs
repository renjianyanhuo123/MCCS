namespace MCCS.Models.Hardwares
{
    public class HardwareSignalBindDevicesItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        private bool _isBinding;
        public bool IsBinding
        {
            get => _isBinding;
            set => SetProperty(ref _isBinding, value);
        }
    }

    public class HardwareSignalListItemViewModel : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _tempId; 
        public string TempId
        {
            get => _tempId;
            set => SetProperty(ref _tempId, value);
        }

        private bool _isCanEdit;
        public bool IsCanEdit
        {
            get => _isCanEdit;
            set => SetProperty(ref _isCanEdit, value);
        }

        private bool _isAdded;
        public bool IsAdded
        {
            get => _isAdded;
            set => SetProperty(ref _isAdded, value);
        }

        private string _signalName;
        public string SignalName
        {
            get => _signalName;
            set => SetProperty(ref _signalName, value);
        }

        private long _address;
        public long Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private int _signalRoleType;
        public int SignalRoleType
        {
            get => _signalRoleType;
            set => SetProperty(ref _signalRoleType, value);
        }

        private int _dataType;
        public int DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }

        private double _weightCoefficient;
        public double WeightCoefficient
        {
            get => _weightCoefficient;
            set => SetProperty(ref _weightCoefficient, value);
        }

        private double _updateCycle;
        public double UpdateCycle
        {
            get => _updateCycle;
            set => SetProperty(ref _updateCycle, value);
        }

        private double _upLimitRange;
        public double UpLimitRange
        {
            get => _upLimitRange;
            set => SetProperty(ref _upLimitRange, value);
        }

        private double _downLimitRange;
        public double DownLimitRange
        {
            get => _downLimitRange;
            set => SetProperty(ref _downLimitRange, value);
        }

        private HardwareSignalBindDevicesItemViewModel _connectedDevice;
        public HardwareSignalBindDevicesItemViewModel ConnectedDevice
        {
            get => _connectedDevice;
            set => SetProperty(ref _connectedDevice, value);
        }
    }
}
