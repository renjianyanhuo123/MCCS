using MahApps.Metro.Controls;

namespace MCCS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            HamburgerMenuControl.Width = 48;
        }

        private void HamburgerMenuControl_HamburgerButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuControl.Width = HamburgerMenuControl.IsPaneOpen ? 48 : 200;
            //var t = HamburgerMenuControl.ActualHeight;
        }
    }
}