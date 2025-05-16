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
            UpdatePageSizeUi(DefaultPageSize);
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

        public int DefaultPageSize
        {
            get => (int)GetValue(DefaultPageSizeProperty);
            set => SetValue(DefaultPageSizeProperty, value);
        }

        public static readonly DependencyProperty DefaultCurrentPageProperty =
            DependencyProperty.Register(nameof(DefaultCurrentPage),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(1, OnDefaultCurrentPageChanged));

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

        public static DependencyProperty DefaultPageSizeProperty =
            DependencyProperty.Register(nameof(DefaultPageSize),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(10, OnDefaultPageSizeChanged));

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

        private static void OnDefaultPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int defaultPageSize) return;
            control.UpdatePageSizeUi(defaultPageSize);
        }

        private static void OnTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int total) return;
            control.UpdateTotalUi(total);
            // var viewModel = control.DataContext as PaginationViewModel;
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

        private void UpdatePageSizeUi(int pageSize)
        {
            SelectPageSize.SelectedIndex = pageSize / 10 - 1;
        }

        #endregion
    }
}
