using MCCS.ViewModels.Pages.SystemManager;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace MCCS.Views.Pages.SystemManager
{
    /// <summary>
    /// HardwareSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareSettingPage
    {

        public HardwareSettingPage()
        {
            InitializeComponent();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: long id } && DataContext is HardwareSettingPageViewModel vm)
                {
                    await vm.EditHardwareCommand.Execute(id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ExMessage}", ex.Message); 
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: long id } && DataContext is HardwareSettingPageViewModel vm)
                {
                    await vm.DeleteHardwareCommand.Execute(id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ExMessage}", ex.Message);
            }
        }

        private async void EditSignalButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: long id } && DataContext is HardwareSettingPageViewModel vm)
                {
                    await vm.EditSignalCommand.Execute(id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ExMessage}", ex.Message);
            }
        }
    }
}
