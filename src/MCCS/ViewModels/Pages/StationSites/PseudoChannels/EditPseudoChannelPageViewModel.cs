using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.Models.Stations.PseudoChannels;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.StationSites;
using MCCS.Events.StationSites.PseudoChannels;

namespace MCCS.ViewModels.Pages.StationSites.PseudoChannels
{
    public sealed class EditPseudoChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "EditPseudoChannelPage";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private long _stationId = -1;
        private long _channelId = -1;

        public EditPseudoChannelPageViewModel(IEventAggregator eventAggregator,
            IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _eventAggregator.GetEvent<SendEditPseudoChannelStationSiteIdEvent>().Subscribe(param =>
            {
                _stationId = param.StationId;
                _channelId = param.ChannelId;
            });
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

        private double _rangeMin;
        public double RangeMin { get => _rangeMin; set => SetProperty(ref _rangeMin, value); }

        private double _rangeMax;
        public double RangeMax { get => _rangeMax; set => SetProperty(ref _rangeMax, value); }

        /// <summary>
        /// 单位
        /// </summary>
        private string _unit;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        private string _formula = string.Empty;
        public string Formula { get => _formula; set => SetProperty(ref _formula, value); }

        private bool _hasTare;
        public bool HasTare { get => _hasTare; set => SetProperty(ref _hasTare, value); }

        public ObservableCollection<PseudoChannelSelectableItemModel> SelectableSignals { get; private set; } = [];
        public ObservableCollection<EditPseudoChannelBindedSignalItemModel> SignalModels { get; private set; } = [];
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
            SignalModels.Add(new EditPseudoChannelBindedSignalItemModel
            {
                TempId = Guid.NewGuid().ToString("N")
            });
        }

        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1 || _channelId == -1) throw new ArgumentNullException("StationId or ChannelId is invalid!");
            SelectableSignals.Clear();
            SignalModels.Clear();

            var pseudoChannel = await _stationSiteAggregateRepository.GetPseudoChannelById(_channelId);
            var staionSelectedHardware = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId);
            var allDevices = await _deviceInfoRepository.GetAllDevicesAsync();
            var signals = await _stationSiteAggregateRepository.GetPseudoChannelAndSignalInfosAsync(c => c.PseudoChannelId == _channelId);

            ChannelName = pseudoChannel?.ChannelName ?? string.Empty;
            InternalId = pseudoChannel?.ChannelId ?? string.Empty;
            RangeMin = pseudoChannel?.RangeMin ?? 0;
            RangeMax = pseudoChannel?.RangeMax ?? 0;
            Formula = pseudoChannel?.Formula ?? string.Empty;
            HasTare = pseudoChannel?.HasTare ?? false;
            Unit = pseudoChannel?.Unit ?? string.Empty;
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

            foreach (var signal in signals)
            {
                var selectedSignal = SelectableSignals.FirstOrDefault(c => c.SignalId == signal.SignalId);
                SignalModels.Add(new EditPseudoChannelBindedSignalItemModel
                {
                    Id = signal.Id,
                    TempId = Guid.NewGuid().ToString("N"),
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
            var pseudoChannelInfo = new PseudoChannelInfo()
            {
                Id = _channelId,
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
                    PseudoChannelId = _channelId,
                    SignalId = signal.SelectedSignalModel?.SignalId ?? 0
                }).ToList();
            var success = await _stationSiteAggregateRepository.UpdateStationSitePseudoChannelAsync(pseudoChannelInfo, signals);
            if (success)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationUpdatePseudoChannelEvent>()
                    .Publish(new NotificationUpdatePseudoChannelEventParam
                    {
                        PseudoChannelId = pseudoChannelInfo.Id
                    });
            }
        }
        #endregion
    }
}
