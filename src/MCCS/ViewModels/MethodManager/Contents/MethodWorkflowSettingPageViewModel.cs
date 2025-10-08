using System.Windows.Controls;
using MahApps.Metro.Controls;
using MCCS.Events.Common;
using MCCS.ViewModels.Pages.WorkflowSteps;
using MCCS.WorkflowSetting;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodWorkflowSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodWorkflowSettingPage";

        private long _methodId = -1;
        private readonly IWorkflowCanvasRenderer _workflowCanvasRenderer;
        private readonly IRegionManager _regionManager;

        public MethodWorkflowSettingPageViewModel(IEventAggregator eventAggregator,
            IWorkflowCanvasRenderer workflowCanvasRenderer,
            IRegionManager regionManager) : base(eventAggregator)
        {
            _workflowCanvasRenderer = workflowCanvasRenderer;
            _regionManager = regionManager;
            LoadCommand = new DelegateCommand<object>(ExecuteLoadCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }

        #region Command 
        public DelegateCommand<object> LoadCommand { get; }
        #endregion

        #region Private Method

        private void ExecuteShowSteps()
        {
            _eventAggregator.GetEvent<OpenRightFlyoutEvent>().Publish(new OpenRightFlyoutEventParam
            {
                Title = "添加处理节点",
                Type = RightFlyoutTypeEnum.WorkflowSetting
            });
            _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(WorkflowStepListPageViewModel.Tag, UriKind.Relative));
        }

        private void ExecuteLoadCommand(object param)
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!"); 
            if (param is Canvas designCanvas)
            {
                var graph = new WorkflowGraph(designCanvas.Width, designCanvas.Height, ExecuteShowSteps);
                _workflowCanvasRenderer.Initialize(designCanvas);
                _workflowCanvasRenderer.RenderWorkflow(graph);
            }
        }
        #endregion
    }
}
