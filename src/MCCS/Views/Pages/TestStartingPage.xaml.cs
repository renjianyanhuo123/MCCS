using System.Windows.Controls;
using System.Windows.Input;

namespace MCCS.Views.Pages
{
    /// <summary>
    /// TestStartingPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestStartingPage : UserControl
    { 

        public TestStartingPage()
        {
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var viewModel = DataContext as ViewModels.Pages.TestStartingPageViewModel;
            await viewModel?.LoadModelsCommand.Execute()!;
        }

        /// <summary>
        /// 处理鼠标点击事件，选中模型并更新状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton != MouseButton.Left) return;
            //var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

            //// 重置所有模型状态
            ////_viewModel.ResetAllModels();

            //// 处理选中事件
            //if (hitResult is RayMeshGeometry3DHitTestResult { ModelHit: { } hitModel })
            //{
            //    var selectedVm = FindViewModel(hitModel);
            //    //_viewModel.SelectedModel = selectedVm;
            //}
            //else
            //{
            //    //_viewModel.SelectedModel = null;
            //}
        }

        /// <summary>
        /// 鼠标悬停事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            //return;
            //// 处理鼠标悬停事件
            //var hitResult = VisualTreeHelper.HitTest(viewPort, e.GetPosition(viewPort));

            //// 重置所有模型的悬停状态
            //foreach (var modelVm in _viewModel.Model3DList)
            //{
            //    modelVm.IsHovered = false;
            //}

            //// 设置当前悬停模型状态
            //if (hitResult is not RayMeshGeometry3DHitTestResult { ModelHit: { } hitModel }) return;
            //var hoveredVm = FindViewModel(hitModel);
            //hoveredVm.IsHovered = true;
        }  
    }
}
