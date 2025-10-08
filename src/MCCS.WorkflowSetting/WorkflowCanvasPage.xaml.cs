namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// WorkflowCanvasPage.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowCanvasPage
    {
        public WorkflowCanvasPage()
        {
            DataContext = new WorkflowCanvasPageViewModel();
            InitializeComponent(); 
        }
    }
}
