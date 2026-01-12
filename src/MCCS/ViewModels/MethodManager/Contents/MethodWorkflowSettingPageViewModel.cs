using System.Windows.Controls;

using MCCS.Common.Resources.ViewModels;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.WorkflowSetting.EventParams;
using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodWorkflowSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodWorkflowSettingPage";
        private readonly IMethodRepository _methodRepository;
        private readonly IWorkflowSerializer _workflowSerializer;

        private long _methodId = -1;
        private double _oldWidth;
        private double _oldHeight;

        public MethodWorkflowSettingPageViewModel(IEventAggregator eventAggregator,
            IMethodRepository methodRepository,
            IWorkflowSerializer workflowSerializer,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _methodRepository = methodRepository;
            _workflowSerializer = workflowSerializer;
            LoadCommand = new AsyncDelegateCommand<object>(ExecuteLoadCommand);
            _eventAggregator.GetEvent<NotificationWorkflowChangedEvent>().Subscribe(OnNotificationWorkflowChangedEvent);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        //public override bool IsNavigationTarget(NavigationContext navigationContext) => false;
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var json = _workflowSerializer.Serialize(WorkflowNodes);
            _methodRepository.AddWorkflowSetting(new MethodWorkflowSettingModel
            {
                MethodId = _methodId,
                WorkflowSetting = json
            });
        }

        #region Property
        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _perect = 100;
        public int Perect
        {
            get => _perect;
            set
            {
                if (SetProperty(ref _perect, value))
                {
                    Scale = _perect / 100.0;
                }
            }
        }

        private StepListNodes _workflowNodes; 
        public StepListNodes WorkflowNodes
        {
            get => _workflowNodes;
            set => SetProperty(ref _workflowNodes, value);
        }

        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        #endregion

        #region Command 
        public AsyncDelegateCommand<object> LoadCommand { get; }  
        #endregion

        #region Private Method
        private void OnNotificationWorkflowChangedEvent(NotificationWorkflowChangedEventParam param)
        { 
            Height = param.Height + 100;
            if (Height < _oldHeight)
            {
                Height = _oldHeight;
            } 
            Width = param.Width + 100;
            if (Width < _oldWidth)
            {
                Width = _oldWidth;
            }
        }

        private async Task ExecuteLoadCommand(object param)
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");
            if (param is Grid element)
            {
                _oldWidth = element.ActualWidth;
                _oldHeight = element.ActualHeight;
                Width = element.ActualWidth;
                Height = element.ActualHeight; 
            }  
            var workflowSettingModel = await _methodRepository.GetMethodWorkflowSettingAsync(_methodId);
            if (workflowSettingModel?.WorkflowSetting == null)
            {
                var temp = new StepListNodes(_eventAggregator, _dialogService,[
                    new StartNode(),
                    new AddOpNode(null),
                    new EndNode()
                ]); 
                WorkflowNodes = temp;
            }
            else
            { 
                WorkflowNodes = _workflowSerializer.Deserialize(workflowSettingModel.WorkflowSetting, _eventAggregator, _dialogService);
            }
        }
        #endregion
    }
}
