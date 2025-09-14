using System.Windows;

namespace MCCS.Views.Pages
{
    /// <summary>
    /// MainContentPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainContentPage
    {
        public MainContentPage()
        {
            InitializeComponent();
            HamburgerMenuControl.Width = 48;
        }

        private void HamburgerMenuControl_HamburgerButtonClick(object sender, RoutedEventArgs e)
        {
            HamburgerMenuControl.Width = HamburgerMenuControl.IsPaneOpen ? 48 : 200; 
        }
    }
}
