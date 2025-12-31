using MaterialDesignThemes.Wpf;

using MCCS.Common.Resources.ViewModels;
using MCCS.Events.Hardwares;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Repositories;

namespace MCCS.ViewModels.Dialogs.Hardwares
{
    public class EditHardwareDialogViewModel:BaseViewModel
    {
        public const string Tag = "EditHardwareDialog";

        private long _hardwareId = -1;
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public EditHardwareDialogViewModel(IEventAggregator eventAggregator,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
            _eventAggregator.GetEvent<SendHardwareIdEvent>().Subscribe(param =>
            {
                _hardwareId = param.HardwareId;
            });
        } 

        #region Property
        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        private int _deviceType;
        public int DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        private string _desprition = string.Empty;
        public string Desprition
        {
            get => _desprition;
            set => SetProperty(ref _desprition, value);
        }

        private int _functionType;
        public int FunctionType
        {
            get => _functionType;
            set => SetProperty(ref _functionType, value);
        }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        #endregion

        #region Private Method
        private async Task ExecuteLoadCommand()
        { 
            var hardware = await _deviceInfoRepository.GetDeviceByIdAsync(_hardwareId);
            DeviceName = hardware.DeviceName;
            DeviceType = (int)hardware.DeviceType;
            Desprition = hardware.Description ?? "";
            FunctionType = (int)hardware.FunctionType;
        }

        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }
        private async Task ExecuteSaveCommand()
        {
            var success = await _deviceInfoRepository.UpdateDeviceInfoAsync(new DeviceInfo()
            {
                Id = _hardwareId,
                DeviceId = Guid.NewGuid().ToString("N"),
                DeviceName = DeviceName,
                Description = Desprition,
                DeviceType = (DeviceTypeEnum)DeviceType,
                FunctionType = (FunctionTypeEnum)FunctionType
            });
            if (success)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationEditHardwareEvent>()
                    .Publish(new NotificationEditHardwareEventParam());
            }
        }

        #endregion

    }
}
