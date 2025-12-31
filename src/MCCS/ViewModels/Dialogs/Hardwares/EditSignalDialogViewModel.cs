using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;

using MCCS.Common.Resources.ViewModels;
using MCCS.Events.Hardwares;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Repositories;
using MCCS.Models.Hardwares;

namespace MCCS.ViewModels.Dialogs.Hardwares
{
    public class EditSignalDialogViewModel : BaseViewModel
    {
        public const string Tag = "EditSignalDialog";

        private long _controllerId = -1;

        private readonly IDeviceInfoRepository _deviceInfoRepository;
        public EditSignalDialogViewModel(IEventAggregator eventAggregator,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            AddressItems =
            [
                new AddressItem { Value = 0, Display = "AI_0" },
                new AddressItem { Value = 1, Display = "AI_1" },
                new AddressItem { Value = 2, Display = "AI_2" },
                new AddressItem { Value = 3, Display = "AI_3" },
                new AddressItem { Value = 4, Display = "AI_4" },
                new AddressItem { Value = 5, Display = "AI_5" },
                new AddressItem { Value = 10, Display = "SSI_0" },
                new AddressItem { Value = 11, Display = "SSI_1" }
            ];
            _eventAggregator.GetEvent<SendHardwareSignalIdEvent>().Subscribe(param => _controllerId = param.ControllerId);
        }

        #region Property 
        public ObservableCollection<AddressItem> AddressItems { get; set; }
        public ObservableCollection<HardwareSignalListItemViewModel> Signals { get; private set; } = [];
        public ObservableCollection<HardwareSignalBindDevicesItemViewModel> BindDevices { get; private set; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public DelegateCommand AddSignalContainerCommand => new(ExecuteAddSignalContainerCommand);
        public DelegateCommand<string> EditSignalCommand => new(ExecuteEditSignalCommand);
        public AsyncDelegateCommand<string> EditOrAddCommand => new(ExecuteEditOrAddCommand);
        public DelegateCommand<string> CancelOrDeleteCommand => new(ExecuteCancelOrDeleteCommand);
        public AsyncDelegateCommand<long> DeleteCommand => new(ExecuteDeleteCommand);
        #endregion

        #region Private Method 
        private async Task ExecuteDeleteCommand(long id)
        {
            var temp = Signals.FirstOrDefault(c => c.Id == id);
            if (temp == null) return;
            Signals.Remove(temp);
            var success = await _deviceInfoRepository.DeleteSignalInfoAsync(id);
        }

        private void ExecuteCancelOrDeleteCommand(string id)
        {
            var temp = Signals.FirstOrDefault(c => c.TempId == id);
            if (temp == null) return;
            if (temp.IsAdded)
            {
                temp.IsCanEdit = false;
            }
            else
            {
                Signals.Remove(temp);
            }
        }

        private async Task ExecuteEditOrAddCommand(string id)
        {
            if (_controllerId == -1) throw new ArgumentNullException("no controllerId");
            var temp = Signals.FirstOrDefault(c => c.TempId == id);
            if (temp == null) return;
            if (temp.IsAdded)
            {
                bool success = await _deviceInfoRepository.UpdateSignalInfoAsync(new SignalInterfaceInfo()
                {
                    Id = temp.Id,
                    Unit = "",
                    SignalAddress = temp.Address,
                    DataType = (SignalDataTypeEnum)temp.DataType,
                    SignalName = temp.SignalName,
                    DownLimitRange = temp.DownLimitRange,
                    UpLimitRange = temp.UpLimitRange,
                    SignalRole = (SignalRoleTypeEnum)temp.SignalRoleType,
                    BelongToControllerId = _controllerId,
                    WeightCoefficient = temp.WeightCoefficient,
                    UpdateCycle = temp.UpdateCycle,
                    ConnectedDeviceId = temp.ConnectedDevice?.Id ?? 0
                });
                if (success)
                {
                    temp.IsCanEdit = false;
                }
            }
            else
            {
                var newId = await _deviceInfoRepository.AddSignalInfoAsync(new SignalInterfaceInfo()
                {
                    SignalAddress = temp.Address,
                    Unit = "",
                    DataType = (SignalDataTypeEnum)temp.DataType,
                    SignalName = temp.SignalName,
                    DownLimitRange = temp.DownLimitRange,
                    UpLimitRange = temp.UpLimitRange,
                    SignalRole = (SignalRoleTypeEnum)temp.SignalRoleType,
                    BelongToControllerId = _controllerId,
                    WeightCoefficient = temp.WeightCoefficient,
                    UpdateCycle = temp.UpdateCycle,
                    ConnectedDeviceId = temp.ConnectedDevice?.Id ?? 0
                });
                if (newId > 0)
                {
                    temp.Id = newId;
                    temp.IsCanEdit = false;
                    temp.IsAdded = true;
                }
            } 
            var bindeddevice = BindDevices
                .FirstOrDefault(c => c.Id == temp.ConnectedDevice?.Id);
            if (bindeddevice != null)
            {
                bindeddevice.IsBinding = true;
            }
        }

        private void ExecuteEditSignalCommand(string id)
        {
            var temp = Signals.FirstOrDefault(c => c.TempId == id);
            if (temp != null)
            {
                temp.IsCanEdit = true;
                temp.IsAdded = true;
            }
        }

        private void ExecuteAddSignalContainerCommand()
        {
            Signals.Add(new HardwareSignalListItemViewModel()
            {
                TempId = Guid.NewGuid().ToString("N"),
                IsCanEdit = true,
                IsAdded = false
            });
        }

        private async Task ExecuteLoadCommand()
        {
            if (_controllerId == -1) throw new ArgumentNullException("no controllerId");
            var signals =
                await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c =>
                    c.BelongToControllerId == _controllerId && c.IsDeleted == false);
            var bindDevices = await _deviceInfoRepository.GetDevicesByExpressionAsync(c =>
                c.FunctionType == FunctionTypeEnum.Implement || c.FunctionType == FunctionTypeEnum.Measurement && c.IsDeleted == false);
            var allBindDevices = signals.Select(s => s.ConnectedDeviceId).ToList();
            BindDevices.Clear();
            foreach (var bindDevice in bindDevices)
            {
                BindDevices.Add(new HardwareSignalBindDevicesItemViewModel
                {
                    Id = bindDevice.Id,
                    DeviceName = bindDevice.DeviceName,
                    IsBinding = allBindDevices.Any(c => c == bindDevice.Id)
                });
            }
            Signals.Clear();
            foreach (var signal in signals)
            {
                Signals.Add(new HardwareSignalListItemViewModel()
                {
                    Id = signal.Id,
                    TempId = Guid.NewGuid().ToString("N"),
                    Address = signal.SignalAddress,
                    DataType = (int)signal.DataType,
                    SignalName = signal.SignalName,
                    IsCanEdit = false,
                    IsAdded = true,
                    DownLimitRange = signal.DownLimitRange,
                    UpLimitRange = signal.UpLimitRange,
                    SignalRoleType = (int)signal.SignalRole,
                    UpdateCycle = signal.UpdateCycle,
                    WeightCoefficient = signal.WeightCoefficient,
                    ConnectedDevice = BindDevices.FirstOrDefault(c => c.Id == signal.ConnectedDeviceId)
                });
            }
        }
        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog", null);
        }
        #endregion
    }
}
