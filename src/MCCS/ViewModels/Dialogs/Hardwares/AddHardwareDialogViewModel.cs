using MaterialDesignThemes.Wpf; 
using MCCS.Events.Hardwares;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Repositories;

namespace MCCS.ViewModels.Dialogs.Hardwares
{
    public class AddHardwareDialogViewModel:BaseViewModel
    {
        public const string Tag = "AddHardwareDialog";

        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public AddHardwareDialogViewModel(IEventAggregator eventAggregator,
            IDeviceInfoRepository deviceInfoRepository) : base(eventAggregator)
        {
            _deviceInfoRepository = deviceInfoRepository;
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
        public DelegateCommand CloseCommand => new(ExecuteCloseCommand);
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        #endregion

        #region Private Method
        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }
        private async Task ExecuteSaveCommand()
        {
            long addId = await _deviceInfoRepository.AddDeviceAsync(new DeviceInfo()
            {
                DeviceId = Guid.NewGuid().ToString("N"),
                DeviceName = DeviceName,
                Description = Desprition,
                DeviceType = (DeviceTypeEnum)DeviceType,
                FunctionType = (FunctionTypeEnum)FunctionType
            });
            if (addId > 0)
            {
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationAddHardwareEvent>()
                    .Publish(new NotificationAddHardwareEventParam()
                    { 
                        HardwareId = addId
                    });
            }
        }

        #endregion

    }
}
