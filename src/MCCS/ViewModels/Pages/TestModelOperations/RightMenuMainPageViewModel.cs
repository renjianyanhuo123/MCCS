using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.Devices;
using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Events.Tests;

namespace MCCS.ViewModels.Pages.TestModelOperations
{
    public sealed class RightMenuMainPageViewModel : BaseViewModel
    {
        public const string Tag = "RightMenuMainPage";
        private long _deviceId = -1;


        public RightMenuMainPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            OpenValveCommand = new DelegateCommand(ExecuteOpenValveCommand);
            eventAggregator.GetEvent<NotificationRightMenuValveStatusEvent>()
                .Subscribe(param =>
                {
                    _deviceId = param.DeviceId;
                    var device = GlobalDataManager.Instance.Devices?.FirstOrDefault(c => c.Id == param.DeviceId);
                    if (device is { Type: DeviceTypeEnum.Actuator } and ActuatorDevice actuatorDevice)
                    {
                        IsOpen = actuatorDevice.ValveStatus == ValveStatusEnum.Opened;
                    }
                });
        }
         
        #region Property 
        private bool _isOpen = false; 
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }
        #endregion

        #region Command 
        public DelegateCommand OpenValveCommand { get; }
        #endregion

        #region Private Method
        private void ExecuteOpenValveCommand()
        {
            if (_deviceId == -1) return;
            var device = GlobalDataManager.Instance.Devices?.FirstOrDefault(s => s.Id == _deviceId);
            if (device is ActuatorDevice actuatorDevice)
            {
                actuatorDevice.OpenValve();
                IsOpen = true;
            }
        }

        #endregion
    }
}
