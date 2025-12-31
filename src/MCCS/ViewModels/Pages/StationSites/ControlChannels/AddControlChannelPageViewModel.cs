using MCCS.Events.StationSites;
using MCCS.Models.Stations.ControlChannels;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

using MCCS.Common.Resources.ViewModels;
using MCCS.Events.StationSites.ControlChannels;
using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Models.StationSites;
using MCCS.Infrastructure.Repositories;
using MCCS.Models.ControlCommand;

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
        public ObservableCollection<ControlChannelBindControllerItemModel> BindControllerInfos { get; } = [];
         
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
        public ObservableCollection<AddControlChannelFeedbackSignalItemModel> SignalModels { get; private set; } = [];

        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand<SelectionChangedEventArgs> SelectionChangedCommand => new(ExecuteSelectionChangedCommand);
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        public DelegateCommand AddSignalCommand => new(ExecuteAddSignalCommand);
        public DelegateCommand<string> DeleteSignalCommand => new(ExecuteDeleteSignalCommand);
        #endregion

        #region Private Method
        private void ExecuteDeleteSignalCommand(string id)
        {
            var removeObj = SignalModels.FirstOrDefault(c => c.TempId == id);
            if(removeObj != null) SignalModels.Remove(removeObj);
        }

        private void ExecuteAddSignalCommand()
        {
            SignalModels.Add(new AddControlChannelFeedbackSignalItemModel
            {
                TempId = Guid.NewGuid().ToString("N"),
                SignalType = 0
            });
        }

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
                ChannelType = (ChannelTypeEnum)ChannelType,
                IsShowable = IsShowable,
                IsOpenSpecimenProtected = IsOpenSpecimenProtected
            };
            var signals = SignalModels
                .Select(signal => new ControlChannelAndSignalInfo
                {
                    DeviceId = signal.SelectedSignalModel?.DeviceId ?? 0, 
                    SignalId = signal.SelectedSignalModel?.SignalId ?? 0, 
                    SignalType = (SignalTypeEnum)signal.SignalType
                }).ToList();
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
            BindControllerInfos.Clear();
            var staionSelectedHardware = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId); 
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync(); 
            foreach (var device in allDevices)
            {
                if (device.DeviceType != DeviceTypeEnum.Controller) continue;
                BindControllerInfos.Add(new ControlChannelBindControllerItemModel
                {
                    DeviceId = device.Id,
                    DeviceName = device.DeviceName
                });
            }
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
        }
        #endregion
    }
}
