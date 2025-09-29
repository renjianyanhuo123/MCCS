using MCCS.ViewModels.MethodManager;
using MCCS.ViewModels.Pages.SystemManager;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace MCCS.Views.MethodManager
{
    /// <summary>
    /// MethodMainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MethodMainPage
    {
        public MethodMainPage()
        {
            InitializeComponent();
        }

        private async void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag is not long methodId) return;
                var viewModel = DataContext as MethodMainPageViewModel;
                // 处理业务逻辑
                await viewModel?.DeleteMethodCommand.Execute(methodId)!;
            }
            catch (Exception ex)
            {
                Log.Error($"Change Station Status Failed! {ex.Message}");
            }
        }
    }
}
