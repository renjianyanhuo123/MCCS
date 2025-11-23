using MCCS.Collecter.ControlChannelManagers;
using MCCS.Events.Tests;
using MCCS.Infrastructure.TestModels;

namespace MCCS.ViewModels.Pages.TestModelOperations
{
    public sealed class RightMenuMainPageViewModel : BaseViewModel
    {
        public const string Tag = "RightMenuMainPage";
        private long _controlChannelId = -1;
        private string _modelId = string.Empty;

        private readonly IEventAggregator _eventAggregator; 
        private readonly IControlChannelManager _controlChannelManager;

        public RightMenuMainPageViewModel(IEventAggregator eventAggregator,
            IControlChannelManager controlChannelManager) : base(eventAggregator)
        {
            _eventAggregator = eventAggregator; 
            _controlChannelManager = controlChannelManager;
            OperationValveCommand = new DelegateCommand<string>(ExecuteOperationValveCommand);
            eventAggregator.GetEvent<NotificationRightMenuValveStatusEvent>()
                .Subscribe(param =>
                {
                    _controlChannelId = param.ControlChannelId;
                    _modelId = param.ModelId;
                    IsOpen = _controlChannelManager.GetControlChannel(_controlChannelId).ValveStatus == ValveStatusEnum.Opened;
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
            if (_controlChannelId == -1 && string.IsNullOrEmpty(_modelId)) return;
            var success = bool.TryParse(obj, out var isOpen);
            if (!success) return;
            // if (!_controllerManager.OperationSigngleValve((long)actuatorDevice.ParentDeviceId, isOpen)) return;
            // actuatorDevice.OperationValve(isOpen);
            var channel = _controlChannelManager.GetControlChannel(_controlChannelId);
            success = channel.OperationValve(isOpen);
            IsOpen = isOpen;
            if (success)
            {
                _eventAggregator.GetEvent<OperationValveEvent>().Publish(new OperationValveEventParam
                {
                    ModelId = _modelId,
                    IsOpen = isOpen
                });
            }
        }  
        #endregion
    }
}
