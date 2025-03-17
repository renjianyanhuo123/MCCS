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
            if (HamburgerMenuControl.IsPaneOpen)
            {
                HamburgerMenuControl.Width = 48;
            }
            else 
            {
                HamburgerMenuControl.Width = 200;
            }
            //var t = HamburgerMenuControl.ActualHeight;
        }
    }
}