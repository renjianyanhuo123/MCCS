using MCCS.Core.Helper;
using MCCS.Core.Repositories;
using MCCS.Events.StationSites;
using MCCS.Models.Stations.ControlChannels;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.StationSites;
using MCCS.Events.StationSites.ControlChannels;

namespace MCCS.ViewModels.Pages.StationSites.ControlChannels
{
    public sealed class AddControlChannelPageViewModel: BaseViewModel
    {
        public const string Tag = "AddControlChannelPage";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private long _stationId = -1;

        public AddControlChannelPageViewModel(IEventAggregator eventAggregator,
            IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _eventAggregator.GetEvent<SendStationSiteIdEvent>().Subscribe(param => _stationId = param.StationId);
            InternalId = $"ChannelId_{HighPerformanceRandomHash.GenerateRandomHash6()}";
        } 

        #region Property
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
        private ControlChannelSelectableItemModel _positionFeedback;
        public ControlChannelSelectableItemModel PositionFeedback
        {
            get => _positionFeedback;
            set
            {
                if (value.IsSelected) return;
                if (_positionFeedback != null) _positionFeedback.IsSelected = false;
                value.IsSelected = true;
                SetProperty(ref _positionFeedback, value);
            }
        }

        private ControlChannelSelectableItemModel _forceFeedback; 
        public ControlChannelSelectableItemModel ForceFeedback
        {
            get => _forceFeedback;
            set
            {
                if (value.IsSelected) return;
                if(_forceFeedback != null) _forceFeedback.IsSelected = false;
                value.IsSelected = true;
                SetProperty(ref _forceFeedback, value);
            }
        }

        private ControlChannelSelectableItemModel _outputSignal;
        public ControlChannelSelectableItemModel OutputSignal
        {
            get => _outputSignal;
            set
            {
                if (value.IsSelected) return;
                if (_outputSignal != null) _outputSignal.IsSelected = false;
                value.IsSelected = true;
                SetProperty(ref _outputSignal, value);
            }
        }
        public ObservableCollection<ControlChannelSelectableItemModel> SelectableControlChannels { get; private set; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand<SelectionChangedEventArgs> SelectionChangedCommand => new(ExecuteSelectionChangedCommand);
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        #endregion

        #region Private Method
        private async Task ExecuteSaveCommand()
        {
            var controlChannelInfo = new ControlChannelInfo()
            {
                ChannelId = InternalId,
                ChannelName = ChannelName,
                ControlCycle = ControlCycle,
                ControlMode = (ControlChannelModeTypeEnum)ControlMode,
                OutputLimitation = OutputLimit,
                StationId = _stationId,
                IsShowable = IsShowable,
                IsOpenSpecimenProtected = IsOpenSpecimenProtected
            };
            var signals = new List<ControlChannelAndSignalInfo>
            {
                new()
                {
                    DeviceId = PositionFeedback?.DeviceId ?? 0,
                    SignalId = PositionFeedback?.SignalId ?? 0,
                    SignalType = SignalTypeEnum.Position
                },
                new()
                {
                    DeviceId = ForceFeedback?.DeviceId ?? 0,
                    SignalId = ForceFeedback?.SignalId ?? 0,
                    SignalType = SignalTypeEnum.Force
                },
                new()
                {
                    DeviceId = OutputSignal?.DeviceId ?? 0,
                    SignalId = OutputSignal?.SignalId ?? 0,
                    SignalType = SignalTypeEnum.Output
                }
            };
            var addId = await _stationSiteAggregateRepository.AddStationSiteControlChannelAsync(controlChannelInfo, signals);
            if (addId > 0)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationAddControlChannelEvent>()
                    .Publish(new NotificationAddControlChannelEventParam
                    {
                        ControlChannelId = addId
                    });
            }
        }

        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }

        private void ExecuteSelectionChangedCommand(SelectionChangedEventArgs e)
        {
            //if (PositionFeedback.IsSelected || ForceFeedback.IsSelected || OutputSignal.IsSelected) return;
            //PositionFeedback.IsSelected = true;
            //ForceFeedback.IsSelected = true;
            //OutputSignal.IsSelected = true;
        }

        private async Task ExecuteLoadCommand()
        { 
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!");
            SelectableControlChannels.Clear();
            var staionSelectedHardware = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId); 
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            var allSignals = await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => allDevices.Select(s => s.Id).Contains(c.ConnectedDeviceId));
            foreach (var item in staionSelectedHardware)
            {
                //var parentInfo = allDevices.FirstOrDefault(c => c.);
                //var connectedSignal = allSignals.FirstOrDefault(c => c.ConnectedDeviceId == item.Id);
                //SelectableControlChannels.Add(new ControlChannelSelectableItemModel
                //{
                //    ControllerName = parentInfo?.DeviceName ?? string.Empty,
                //    DeviceName = item.DeviceName,
                //    DeviceId = item.Id,
                //    SignalId = connectedSignal?.Id ?? 0,
                //    SignalName = connectedSignal?.SignalName ?? string.Empty,
                //    IsSelected = false
                //});
            }
        }
        #endregion
    }
}
