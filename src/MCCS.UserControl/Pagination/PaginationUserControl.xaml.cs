using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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
            UpdateShowTotalUi(ShowTotal);
            UpdateTotalUi(Total);
        }

        public bool ShowTotal
        {
            get=>(bool)GetValue(ShowTotalProperty);
            set=> SetValue(ShowTotalProperty, value);
        }

        public int Total
        {
            get => (int)GetValue(TotalProperty);
            set => SetValue(TotalProperty, value);
        }

        public int DefaultCurrentPage
        {
            get => (int)GetValue(DefaultCurrentPageProperty);
            set => SetValue(DefaultCurrentPageProperty, value);
        }

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        public static readonly DependencyProperty DefaultCurrentPageProperty =
            DependencyProperty.Register(nameof(DefaultCurrentPage),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(1, OnCurrentPageChanged));

        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(nameof(Total),
                typeof(int),
                typeof(PaginationUserControl),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender, OnTotalChanged));
        public static readonly DependencyProperty ShowTotalProperty =
            DependencyProperty.Register(nameof(ShowTotal),
                typeof(bool),
                typeof(PaginationUserControl),
                new FrameworkPropertyMetadata(
                    false, 
                    FrameworkPropertyMetadataOptions.AffectsRender, 
                    OnShowTotalChanged));

        public static DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(10));

        #region CallBack
        private static void OnShowTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is bool showTotal) control.UpdateShowTotalUi(showTotal);
        }

        private static void OnDefaultCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is not PaginationUserControl control) return;
            //if (e.NewValue is int total) control.UpdateTotalUi(total);
        }

        private static void OnTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is int total) control.UpdateTotalUi(total);
        }
        #endregion
         
        #region Private Method
        private void UpdateShowTotalUi(bool showTotal)
        {
            TotalStackPanel.Visibility = showTotal ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTotalUi(int total)
        {
            TotalPageText.Text = total.ToString();
        }

        #endregion
    }
}
