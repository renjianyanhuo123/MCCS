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
            Result = null!; // CS8618: 明确告知编译器此属性会被后续赋值
        }

        public IDialogResult Result { get; set; }
    }
}
