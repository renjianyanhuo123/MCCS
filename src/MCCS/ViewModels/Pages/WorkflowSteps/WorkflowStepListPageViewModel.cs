using System.Collections.ObjectModel;
using System.Windows.Media;
using MCCS.Models.MethodManager;
using MCCS.WorkflowSetting;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.ViewModels.Pages.WorkflowSteps
{
    public sealed class WorkflowStepListPageViewModel : BaseViewModel
    {
        public const string Tag = "WorkflowStepListPage";

        private readonly IWorkflowCanvasRenderer _workflowCanvasRenderer;

        public WorkflowStepListPageViewModel(IEventAggregator eventAggregator,
            IWorkflowCanvasRenderer workflowCanvasRenderer) : base(eventAggregator)
        {
            _workflowCanvasRenderer = workflowCanvasRenderer;
            Steps = 
            [
                new WorkflowSettingItemModel()
                {
                    Id = 1,
                    Name = "循环",
                    Description = "添加一个固定次数或在指定条件下结束的循环流程",
                    IconStr = "RecycleVariant"
                },
                new WorkflowSettingItemModel()
                {
                    Id = 2,
                    Name = "分支",
                    Description = "向流程中添加分支，按不同条件分别处理",
                    IconStr = "SourceBranch"
                },
                new WorkflowSettingItemModel()
                {
                    Id = 3,
                    Name = "延时",
                    Description = "暂停运行当前流程，并在到达指定时间后继续执行",
                    IconStr = "TimerSandComplete"
                }
            ];
            SelectStepCommand = new DelegateCommand<WorkflowSettingItemModel>(ExecuteSelectStepCommand);
        }

        #region Property 
        public ObservableCollection<WorkflowSettingItemModel> Steps { get; private set; }
        #endregion

        #region Command 
        public DelegateCommand<WorkflowSettingItemModel> SelectStepCommand { get; }
        #endregion

        #region Private Method 
        private void ExecuteSelectStepCommand(WorkflowSettingItemModel param)
        {
            _workflowCanvasRenderer.AddProcessNodeRender(new StepNode("StepNode", param.Name, new SolidColorBrush(Color.FromRgb(255, 163, 64)), 260, 110));
        }
        #endregion
    }
}
