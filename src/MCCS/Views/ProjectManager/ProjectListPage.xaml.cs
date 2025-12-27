using MCCS.ViewModels.ProjectManager;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace MCCS.Views.ProjectManager
{
    /// <summary>
    /// ProjectMainPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectListPage
    {
        public ProjectListPage()
        {
            InitializeComponent();
        }

        private async void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag is not long projectId) return;
                var viewModel = DataContext as ProjectListPageViewModel;
                await viewModel?.DeleteProjectCommand.Execute(projectId)!;
            }
            catch (Exception ex)
            {
                Log.Error($"Delete Project Failed! {ex.Message}");
            }
        }

        private void TestOperation_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag is not long projectId) return;
                var viewModel = DataContext as ProjectListPageViewModel; 
                viewModel?.TestOperationCommand.Execute(projectId);
            }
            catch (Exception ex)
            {
                Log.Error($"Operation Project Failed! {ex.Message}");
            }
        }
    }
}
