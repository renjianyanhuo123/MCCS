using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Components
{
    /// <summary>
    /// WorkflowStepListNodes.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowStepListNodes
    {
        public WorkflowStepListNodes()
        {
            InitializeComponent();
            DataContext = new StepListNodes
            {
                Type = NodeTypeEnum.StepList
            };
        } 
    }
}
