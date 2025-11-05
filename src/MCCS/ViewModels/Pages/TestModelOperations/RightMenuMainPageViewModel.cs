using MCCS.Collecter.ControllerManagers;
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
        private string _modelId = string.Empty;

        private readonly IEventAggregator _eventAggregator;
        private readonly IControllerManager _controllerService;

        public RightMenuMainPageViewModel(IEventAggregator eventAggregator,
            IControllerManager controllerService) : base(eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _controllerService = controllerService;
            OperationValveCommand = new DelegateCommand<string>(ExecuteOperationValveCommand);
            eventAggregator.GetEvent<NotificationRightMenuValveStatusEvent>()
                .Subscribe(param =>
                {
                    _deviceId = param.DeviceId;
                    _modelId = param.ModelId;
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
        public DelegateCommand<string> OperationValveCommand { get; } 
        #endregion

        #region Private Method

        private void ExecuteOperationValveCommand(string obj)
        {
            if (_deviceId == -1 || string.IsNullOrEmpty(_modelId)) return;
            var success = bool.TryParse(obj, out var isOpen);
            if (!success) return;
            var device = GlobalDataManager.Instance.Devices?.FirstOrDefault(s => s.Id == _deviceId);
            if (device is not ActuatorDevice actuatorDevice || actuatorDevice.ParentDeviceId == null) return;  
            if (!_controllerService.OperationSigngleValve((long)actuatorDevice.ParentDeviceId, isOpen)) return;
            actuatorDevice.OperationValve(isOpen);
            IsOpen = isOpen;
            _eventAggregator.GetEvent<OperationValveEvent>().Publish(new OperationValveEventParam
            {
                ModelId = _modelId,
                IsOpen = isOpen
            });
        }  
        #endregion
    }
}
