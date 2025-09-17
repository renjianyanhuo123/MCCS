using System.Windows;

namespace MCCS.Views.Dialogs
{
    /// <summary>
    /// NonModalDialogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NonModalDialogWindow : IDialogWindow
    {
        public NonModalDialogWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = true;
            Owner = null; // 关键：不设置Owner避免模态
        }

        public IDialogResult Result { get; set; }
    }
}
