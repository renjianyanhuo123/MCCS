using MCCS.WorkflowSetting.Models.Nodes;
using System.Windows;
using MCCS.WorkflowSetting;
using System.Windows.Controls;
using MCCS.WorkflowSetting.Models.Edges;

namespace MCCS.Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        { 
            InitializeComponent();
        }


        private void BtnRender_OnClick(object sender, RoutedEventArgs e)
        {
            var graph = new WorkflowGraph(workflowCanvas.Width, workflowCanvas.Height, () => { });
            // 6. 渲染
            var renderer = new WorkflowCanvasRenderer();
            renderer.Initialize(workflowCanvas);
            renderer.RenderWorkflow(graph);
        } 
    }
}