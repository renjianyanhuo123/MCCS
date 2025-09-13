using System.Windows;
using System.Windows.Controls.Primitives;
using MCCS.ViewModels.Pages.SystemManager;
using Serilog;

namespace MCCS.Views.Pages.SystemManager
{
    /// <summary>
    /// StationSiteSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class StationSiteSettingPage
    {
        public StationSiteSettingPage()
        {
            InitializeComponent();
        }

        private async void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var toggleButton = sender as ToggleButton; 
                if (toggleButton?.Tag is not long station) return;
                var viewModel = DataContext as StationSiteSettingPageViewModel;
                // 处理业务逻辑
                await viewModel?.OperationCheckedCommand.Execute(station)!;
            }
            catch (Exception ex)
            {
                Log.Error($"Change Station Status Failed! {ex.Message}");
            }
        }
    }
}
