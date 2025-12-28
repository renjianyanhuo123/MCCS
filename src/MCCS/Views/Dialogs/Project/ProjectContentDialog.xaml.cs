using System.Windows;
using System.Windows.Input;

namespace MCCS.Views.Dialogs.Project
{
    /// <summary>
    /// ProjectContentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectContentDialog
    {
        public ProjectContentDialog()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }
    }
}
