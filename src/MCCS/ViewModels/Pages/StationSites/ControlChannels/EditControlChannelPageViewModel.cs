using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.Devices;
using MCCS.Core.Models.StationSites;
using MCCS.Core.Repositories;
using MCCS.Events.StationSites.ControlChannels;
using MCCS.Models.ControlCommand;
using MCCS.Models.Stations.ControlChannels;
using System.Collections.ObjectModel;

namespace MCCS.ViewModels.Pages.StationSites.ControlChannels
{
    public sealed class EditControlChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "EditControlChannel";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private long _stationId = -1;
        private long _channelId = -1;

        public EditControlChannelPageViewModel(IEventAggregator eventAggregator, 
            IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _eventAggregator.GetEvent<SendEditChannelStationSiteIdEvent>().Subscribe(param =>
            {
                _stationId = param.StationId;
                _channelId = param.ChannelId;
            });
        }

        #region Property
        public ObservableCollection<ControlChannelBindControllerItemModel> BindControllerInfos { get; } = [];

        private ControlChannelBindControllerItemModel _bindControllerInfo;
        public ControlChannelBindControllerItemModel BindControllerInfo
        {
            get => _bindControllerInfo;
            set => SetProperty(ref _bindControllerInfo, value);
        }

        private string _channelName = string.Empty;
        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        private string _internalId = string.Empty;
        public string InternalId
        {
            get => _internalId;
            set => SetProperty(ref _internalId, value);
        }

        private double _controlCycle;
        public double ControlCycle
        {
            get => _controlCycle;
            set => SetProperty(ref _controlCycle, value);
        }

        private int _controlMode;
        public int ControlMode
        {
            get => _controlMode;
            set => SetProperty(ref _controlMode, value);
        }

        private short _outputLimit;
        public short OutputLimit
        {
            get => _outputLimit;
            set => SetProperty(ref _outputLimit, value);
        }

        private bool _isShowable;
        public bool IsShowable { get => _isShowable; set => SetProperty(ref _isShowable, value); }
        private bool _isOpenSpecimenProtected; 
        public bool IsOpenSpecimenProtected
        {
            get => _isOpenSpecimenProtected;
            set => SetProperty(ref _isOpenSpecimenProtected, value);
        }

        private int _channelType;
        public int ChannelType
        {
            get => _channelType;
            set => SetProperty(ref _channelType, value);
        }
        public ObservableCollection<ControlChannelSelectableItemModel> SelectableControlChannels { get; private set; } = [];
        public ObservableCollection<EditControlChannelFeedbackSignalItemModel> SignalModels { get; private set; } = []; 
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand); 
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        public DelegateCommand AddSignalCommand => new(ExecuteAddSignalCommand);
        public DelegateCommand<string> DeleteSignalCommand => new(ExecuteDeleteSignalCommand);
        #endregion

        #region Private Method
        private void ExecuteDeleteSignalCommand(string id)
        {
            var removeObj = SignalModels.FirstOrDefault(c => c.TempId == id);
            if (removeObj != null) SignalModels.Remove(removeObj);
        }

        private void ExecuteAddSignalCommand()
        {
            SignalModels.Add(new EditControlChannelFeedbackSignalItemModel
            {
                TempId = Guid.NewGuid().ToString("N"),
                SignalType = 0
            });
        }
        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1 || _channelId == -1) throw new ArgumentNullException("StationId or ChannelId is invalid!");
            SelectableControlChannels.Clear();
            SignalModels.Clear();
            BindControllerInfos.Clear();
            var controlChannel = await _stationSiteAggregateRepository.GetControlChannelById(_channelId);
            var staionSelectedHardware = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId);
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            var signals =
                await _stationSiteAggregateRepository.GetControlChannelAndSignalInfosAsync(c =>
                    c.ChannelId == _channelId);
            foreach (var device in allDevices)
            {
                if (device.DeviceType != DeviceTypeEnum.Controller) continue;
                BindControllerInfos.Add(new ControlChannelBindControllerItemModel
                {
                    DeviceId = device.Id,
                    DeviceName = device.DeviceName
                });
            }
            ChannelName = controlChannel?.ChannelName ?? string.Empty;
            InternalId = controlChannel?.ChannelId ?? string.Empty;
            ControlCycle = controlChannel?.ControlCycle ?? 0;
            ControlMode = (int)(controlChannel?.ControlMode ?? 0);
            OutputLimit = controlChannel?.OutputLimitation ?? 0;
            IsShowable = controlChannel?.IsShowable ?? false;
            IsOpenSpecimenProtected = controlChannel?.IsOpenSpecimenProtected ?? false; 
            ChannelType = (int)controlChannel?.ChannelType!;
            BindControllerInfo = BindControllerInfos.FirstOrDefault(c => c.DeviceId == controlChannel.ControllerId);
            foreach (var item in staionSelectedHardware)
            {
                var parentInfo = allDevices.FirstOrDefault(c => c.Id == item.BelongToControllerId);
                var connectedDevice = allDevices.FirstOrDefault(c => c.Id == item.ConnectedDeviceId);
                SelectableControlChannels.Add(new ControlChannelSelectableItemModel
                {
                    ControllerName = parentInfo?.DeviceName ?? string.Empty,
                    DeviceName = connectedDevice?.DeviceName ?? string.Empty,
                    DeviceId = connectedDevice?.Id ?? 0,
                    SignalId = item?.Id ?? 0,
                    SignalName = item?.SignalName ?? string.Empty,
                    IsSelected = false
                });
            }
            // SignalModels
            foreach (var signal in signals)
            {
                var selectedSignal = SelectableControlChannels.FirstOrDefault(c => c.SignalId == signal.SignalId);
                SignalModels.Add(new EditControlChannelFeedbackSignalItemModel
                {
                    Id = signal.Id,
                    TempId = Guid.NewGuid().ToString("N"),
                    SignalType = (int)signal.SignalType,
                    SelectedSignalModel = selectedSignal
                });
            }
        }

        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }

        private async Task ExecuteSaveCommand()
        {
            var controlChannelInfo = new ControlChannelInfo()
            {
                Id = _channelId,
                ChannelId = InternalId,
                ChannelName = ChannelName,
                ControlCycle = ControlCycle,
                ControllerId = BindControllerInfo?.DeviceId ?? 0,
                ControlMode = (ControlChannelModeTypeEnum)ControlMode,
                ChannelType = (ChannelTypeEnum)ChannelType,
                OutputLimitation = OutputLimit,
                StationId = _stationId,
                IsShowable = IsShowable,
                IsOpenSpecimenProtected = IsOpenSpecimenProtected
            };
            var signals = SignalModels
                .Select(signal => new ControlChannelAndSignalInfo
                {
                    ChannelId = _channelId,
                    DeviceId = signal.SelectedSignalModel?.DeviceId ?? 0,
                    SignalId = signal.SelectedSignalModel?.SignalId ?? 0,
                    SignalType = (SignalTypeEnum)signal.SignalType
                }).ToList();
            var success = await _stationSiteAggregateRepository.UpdateStationSiteControlChannelAsync(controlChannelInfo, signals);
            if (success)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationUpdateControlChannelEvent>()
                    .Publish(new NotificationUpdateControlChannelEventParam
                    {
                        ControlChannelId = controlChannelInfo.Id
                    });
            }
        }

        #endregion
    }
}
