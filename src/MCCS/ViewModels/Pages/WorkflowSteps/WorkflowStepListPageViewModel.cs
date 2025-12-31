using System.Collections.ObjectModel;
using System.Windows.Media;
using MCCS.Models.MethodManager;
using MCCS.WorkflowSetting.EventParams;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.ViewModels.Pages.WorkflowSteps
{
    public sealed class WorkflowStepListPageViewModel : BaseViewModel
    {
        public const string Tag = "WorkflowStepListPage";
         
        public WorkflowStepListPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            Steps = 
            [
                new WorkflowSettingItemModel()
                {
                    Id = 1,
                    Name = "循环",
                    Description = "添加一个固定次数或在指定条件下结束的循环流程",
                    IconStr = "RecycleVariant",
                    StepType = StepTypeEnum.Cycle,
                    IconBackground = new SolidColorBrush(Color.FromRgb(76,125,158))
                },
                new WorkflowSettingItemModel()
                {
                    Id = 2,
                    Name = "分支",
                    Description = "向流程中添加分支，按不同条件分别处理",
                    IconStr = "SourceBranch",
                    StepType = StepTypeEnum.Decision,
                    IconBackground = new SolidColorBrush(Color.FromRgb(22,119,255))
                },
                new WorkflowSettingItemModel()
                {
                    Id = 3,
                    Name = "延时",
                    Description = "暂停运行当前流程，并在到达指定时间后继续执行",
                    IconStr = "TimerSandComplete",
                    StepType = StepTypeEnum.Delay,
                    IconBackground = new SolidColorBrush(Color.FromRgb(76,125,158))
                }
            ];
            SelectStepCommand = new DelegateCommand<WorkflowSettingItemModel>(ExecuteSelectStepCommand);
        }

        private string _sourceId = string.Empty;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var paramters = navigationContext.Parameters.GetValue<AddOpEventParam>("OpEventArgs");
            _sourceId = paramters.Source as string ?? throw new ArgumentNullException(nameof(paramters.Source));
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
            if (param == null) return;
            BaseNode? res = null;
            switch (param.StepType)
            {
                case StepTypeEnum.Cycle:
                case StepTypeEnum.Delay:
                    res = new StepNode(_sourceId, _eventAggregator)
                    {
                        Name = "StepNode",
                        Title = param.Name,
                        TitleBackground = param.IconBackground,
                        Width = 260,
                        Height = 110
                    };
                    break;
                case StepTypeEnum.Decision:
                    res = new DecisionNode(_eventAggregator);
                    break;
            }
            if (res != null) _eventAggregator.GetEvent<AddNodeEvent>().Publish(new AddNodeEventParam
            {
                Source = _sourceId,
                Node = res
            });
        }
        #endregion
    }
}
