using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Components
{
    /// <summary>
    /// WorkflowStartNode.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowStartNode
    {
        public WorkflowStartNode()
        {
            InitializeComponent();
            DataContext = new StartNode();
        }
    }
}
