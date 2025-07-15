using System.Windows;
using System.Windows.Controls;

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
            viewModel?.InitializeDataSubscriptions();
            await viewModel?.LoadModelsCommand.Execute()!;
            viewModel?.InitialCurves();
        }
          
    }
}
