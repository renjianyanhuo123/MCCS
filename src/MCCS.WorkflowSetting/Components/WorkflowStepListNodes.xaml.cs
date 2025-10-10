using MCCS.WorkflowSetting.Models.Nodes;
using System.Windows;
using System.Windows.Input;

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
            DataContext = new StepListNodes();
        } 
    }
}
