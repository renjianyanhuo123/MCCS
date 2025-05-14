using System.Windows;

namespace MCCS.UserControl.ComboBox
{
    /// <summary>
    /// CustomComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class CustomComboBox : System.Windows.Controls.UserControl
    {
        public CustomComboBox()
        {
            InitializeComponent();
        }

        #region Dependency Properties
        //public static readonly DependencyProperty ItemsSourceProperty =
        //    DependencyProperty.Register(nameof(ItemsSource),
        //        typeof(IEnumerable<object>),
        //        typeof(CustomComboBox),
        //        new PropertyMetadata(null));
        #endregion
    }
}
