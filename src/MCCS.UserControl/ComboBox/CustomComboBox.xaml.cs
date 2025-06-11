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

        //public string SuffixText
        //{
        //    get => (string)GetValue(SuffixTextProperty);
        //    set => SetValue(SuffixTextProperty, value);
        //}

        #region Dependency Properties
        //public static readonly DependencyProperty SuffixTextProperty =
        //    DependencyProperty.Register(nameof(SuffixText),
        //        typeof(string),
        //        typeof(CustomComboBox),
        //        new PropertyMetadata(string.Empty, OnSuffixTextPropertyChangedCallback));
        #endregion

    }
}
