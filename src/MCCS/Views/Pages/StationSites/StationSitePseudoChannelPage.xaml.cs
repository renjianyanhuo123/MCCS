using MCCS.ViewModels.Pages.StationSites;
using MCCS.ViewModels.Pages.SystemManager;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace MCCS.Views.Pages.StationSites
{
    /// <summary>
    /// StationSitePseudoChannelPage.xaml 的交互逻辑
    /// </summary>
    public partial class StationSitePseudoChannelPage
    {
        public StationSitePseudoChannelPage()
        {
            InitializeComponent();
        }

        private async void DeletePseudoChannel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: long id } && DataContext is StationSitePseudoChannelPageViewModel vm)
                {
                    await vm.DeletePseudoChannelCommand.Execute(id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ExMessage}", ex.Message);
            }
        }

        private async void EditPseudoChannel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: long id } && DataContext is StationSitePseudoChannelPageViewModel vm)
                {
                    await vm.EditPseudoChannelCommand.Execute(id);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ExMessage}", ex.Message);
            }
        }
    }
}
