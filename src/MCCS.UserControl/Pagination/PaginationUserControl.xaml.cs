using System.Windows;

namespace MCCS.UserControl.Pagination
{
    /// <summary>
    /// PaginationUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PaginationUserControl : System.Windows.Controls.UserControl
    {
        private PaginationViewModel _viewModel;

        public PaginationUserControl()
        {
            InitializeComponent();
            _viewModel = new PaginationViewModel();
            this.DataContext = _viewModel;
        }

        public int Total
        {
            get => (int)GetValue(TotalProperty);
            set => SetValue(TotalProperty, value);
        }

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(1, OnCurrentPageChanged));

        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(nameof(Total),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(1, OnTotalChanged));

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaginationUserControl control)
            {
                // control.UpdateCommands();
            }
        }

        private static void OnTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaginationUserControl control)
            {
                // control.UpdateCommands();
            }
        }
    }
}
