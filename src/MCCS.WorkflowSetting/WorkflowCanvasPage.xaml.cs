using System.Windows;
using System.Windows.Input;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// WorkflowCanvasPage.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowCanvasPage
    {
        public WorkflowCanvasPage()
        { 
            InitializeComponent();
            DataContext = new WorkflowCanvasPageViewModel();
        } 
    }
}
