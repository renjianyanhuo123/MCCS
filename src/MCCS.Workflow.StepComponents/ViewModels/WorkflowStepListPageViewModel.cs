using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

using MCCS.Common.Resources.ViewModels;
using MCCS.Workflow.Contact.Events;
using MCCS.Workflow.Contact.Models;
using MCCS.Workflow.StepComponents.Enums;
using MCCS.Workflow.StepComponents.Registry;

namespace MCCS.Workflow.StepComponents.ViewModels
{
    public sealed class WorkflowStepListPageViewModel : BaseViewModel
    {
        public const string Tag = "WorkflowStepListPage"; 

        private readonly IStepRegistry _stepRegistry;

        public WorkflowStepListPageViewModel(
            IStepRegistry stepRegistry,
            IEventAggregator eventAggregator, 
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            //Steps = 
            //[
            //    new WorkflowSettingItemModel()
            //    {
            //        Id = 1,
            //        Name = "循环",
            //        Description = "添加一个固定次数或在指定条件下结束的循环流程",
            //        IconStr = "RecycleVariant", 
            //        IconBackground = new SolidColorBrush(Color.FromRgb(76,125,158))
            //    },
            //    new WorkflowSettingItemModel()
            //    {
            //        Id = 2,
            //        Name = "分支",
            //        Description = "向流程中添加分支，按不同条件分别处理",
            //        IconStr = "SourceBranch", 
            //        IconBackground = new SolidColorBrush(Color.FromRgb(22,119,255))
            //    },
            //    new WorkflowSettingItemModel()
            //    {
            //        Id = 3,
            //        Name = "延时",
            //        Description = "暂停运行当前流程，并在到达指定时间后继续执行",
            //        IconStr = "TimerSandComplete", 
            //        IconBackground = new SolidColorBrush(Color.FromRgb(76,125,158))
            //    }
            //];
            var converter = new BrushConverter(); 
            _stepRegistry = stepRegistry; 
            foreach (ComponentCategory status in Enum.GetValues(typeof(ComponentCategory)))
            {
                var description = GetDescription(status);
                var steps = _stepRegistry.GetStepsByCategory(status);
                var groupModel = new WorkflowSettingGroupModel
                {
                    GroupName = description,
                    Items = steps.Select(step => new WorkflowSettingItemModel
                    {
                        Id = step.Id,
                        Name = step.Name,
                        DisplayName = step.Name,
                        Description = step.Description,
                        IconStr = step.Icon,
                        IconBackground = step.IconBackground == "" ? new SolidColorBrush(Colors.BlueViolet) :
                            converter.ConvertFromString(step.IconBackground) as Brush ?? new SolidColorBrush()
                    }).ToList()
                };
                GroupModels.Add(groupModel);
            } 
            SelectStepCommand = new DelegateCommand<WorkflowSettingItemModel>(ExecuteSelectStepCommand);
        }

        private string _sourceId = string.Empty;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var paramters = navigationContext.Parameters.GetValue<AddOpEventParam>("OpEventArgs");
            _sourceId = paramters.Source as string ?? throw new ArgumentNullException(nameof(paramters.Source));
        } 

        #region Property 
        public ObservableCollection<WorkflowSettingGroupModel> GroupModels { get; private set; } = [];
        #endregion

        #region Command 
        public DelegateCommand<WorkflowSettingItemModel> SelectStepCommand { get; }
        #endregion

        #region Private Method 
        private void ExecuteSelectStepCommand(WorkflowSettingItemModel param)
        {
            if (param == null) return;
            var nodeInfo = new NodeInfo
            {
                Name = param.Name,
                DisplayType = NodeDisplayTypeEnum.Step,
                Title = param.DisplayName,
                TitleBackground = param.IconBackground.ToString()
            };
            _eventAggregator.GetEvent<AddNodeEvent>().Publish(new AddNodeEventParam
            {
                Source = _sourceId,
                Node = nodeInfo
            });
        }

        private static string GetDescription(Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString(); 
            DescriptionAttribute? attr = field.GetCustomAttribute<DescriptionAttribute>(); 
            return attr?.Description ?? value.ToString();
        }
        #endregion
    }
}
