using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.Models.Stations.PseudoChannels;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.StationSites;
using MCCS.Events.StationSites;
using MCCS.Events.StationSites.PseudoChannels;
using MCCS.Infrastructure.Helper;

namespace MCCS.ViewModels.Pages.StationSites.PseudoChannels
{
    public sealed class AddPseudoChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "AddPseudoChannelPage";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private long _stationId = -1;

        public AddPseudoChannelPageViewModel(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _eventAggregator.GetEvent<SendStationSiteIdEvent>().Subscribe(param => _stationId = param.StationId);
            InternalId = $"PseudoChannelId_{HighPerformanceRandomHash.GenerateRandomHash6()}";
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

        /// <summary>
        /// 范围最小值
        /// </summary>
        private double _rangeMin;
        public double RangeMin { get => _rangeMin; set => SetProperty(ref _rangeMin, value); }

        /// <summary>
        /// 范围最大值
        /// </summary>
        private double _rangeMax;
        public double RangeMax { get => _rangeMax; set => SetProperty(ref _rangeMax, value); }

        /// <summary>
        /// 计算公式
        /// </summary>
        private string _formula = string.Empty;
        public string Formula { get => _formula; set => SetProperty(ref _formula, value); }

        /// <summary>
        /// 单位
        /// </summary>
        private string _unit; 
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        /// <summary>
        /// 是否可校准
        /// </summary>
        private bool _hasTare;
        public bool HasTare { get => _hasTare; set => SetProperty(ref _hasTare, value); }

        public ObservableCollection<PseudoChannelSelectableItemModel> SelectableSignals { get; private set; } = [];
        public ObservableCollection<AddPseudoChannelBindedSignalItemModel> SignalModels { get; private set; } = [];

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
            SignalModels.Add(new AddPseudoChannelBindedSignalItemModel
            {
                TempId = Guid.NewGuid().ToString("N")
            });
        }

        private async Task ExecuteSaveCommand()
        {
            var pseudoChannelInfo = new PseudoChannelInfo()
            {
                ChannelId = InternalId,
                ChannelName = ChannelName,
                RangeMin = RangeMin,
                RangeMax = RangeMax,
                Formula = Formula,
                HasTare = HasTare,
                Unit = Unit,
                StationId = _stationId
            };
            var signals = SignalModels
                .Where(s => s.SelectedSignalModel != null)
                .Select(signal => new PseudoChannelAndSignalInfo
                {
                    SignalId = signal.SelectedSignalModel?.SignalId ?? 0
                }).ToList();
            var addId = await _stationSiteAggregateRepository.AddStationSitePseudoChannelAsync(pseudoChannelInfo, signals);
            if (addId > 0)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationAddPseudoChannelEvent>()
                    .Publish(new NotificationAddPseudoChannelEventParam
                    {
                        PseudoChannelId = addId
                    });
            }
        }

        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }

        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1) throw new ArgumentNullException("StationId is invalid!");
            SelectableSignals.Clear();
            var staionSelectedHardware = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId);
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            foreach (var item in staionSelectedHardware)
            {
                var parentInfo = allDevices.FirstOrDefault(c => c.Id == item.BelongToControllerId);
                var connectedDevice = allDevices.FirstOrDefault(c => c.Id == item.ConnectedDeviceId);
                SelectableSignals.Add(new PseudoChannelSelectableItemModel
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
