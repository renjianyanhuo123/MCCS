using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            DataContext = _viewModel;
            this.Loaded += PaginationUserControl_Loaded;
        }

        #region 回调Command
        public static readonly DependencyProperty CurrentPageChangedCommandProperty =
            DependencyProperty.Register(
                nameof(CurrentPageChangedCommandProperty), 
                typeof(ICommand), 
                typeof(PaginationUserControl), 
                new PropertyMetadata(null));

        // 回调命令：PageSize变化
        public static readonly DependencyProperty PageSizeChangedCommandProperty =
            DependencyProperty.Register(
                nameof(PageSizeChangedCommand), 
                typeof(ICommand), 
                typeof(PaginationUserControl), 
                new PropertyMetadata(null));

        public ICommand PageSizeChangedCommand
        {
            get => (ICommand)GetValue(PageSizeChangedCommandProperty);
            set => SetValue(PageSizeChangedCommandProperty, value);
        }

        public ICommand CurrentPageChangedCommand
        {
            get => (ICommand)GetValue(CurrentPageChangedCommandProperty);
            set => SetValue(CurrentPageChangedCommandProperty, value);
        }
        #endregion

        #region Events
        private void PaginationUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 在组件加载完成后手动更新 ViewModel
            if (DataContext is not PaginationViewModel viewModel) return;
            UpdateShowTotalUi(ShowTotal);
            UpdatePageSizeUi(DefaultPageSize);
            viewModel.UpdateFields(Total, DefaultPageSize, DefaultCurrentPage);
        }
        #endregion

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

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(10, OnPageSizeChanged));

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage),
                typeof(int),
                typeof(PaginationUserControl),
                new PropertyMetadata(1, OnCurrentPageChanged));

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
        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int pageSize) return;
            var viewModel = control.DataContext as PaginationViewModel;
            viewModel.PageSize = pageSize;
            control.PageSizeChangedCommand.Execute(pageSize);
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int currentPage) return;
            var viewModel = control.DataContext as PaginationViewModel;
            viewModel.CurrentPage = currentPage;
            control.CurrentPageChangedCommand.Execute(currentPage);
        }

        private static void OnShowTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is bool showTotal) control.UpdateShowTotalUi(showTotal);
        }

        private static void OnDefaultCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int defaultCurrentPage) return;
            var viewModel = control.DataContext as PaginationViewModel;
            viewModel.CurrentPage = defaultCurrentPage;
        }

        private static void OnDefaultPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int defaultPageSize) return;
            control.UpdatePageSizeUi(defaultPageSize);
            var viewModel = control.DataContext as PaginationViewModel;
            viewModel.PageSize = defaultPageSize;
        }

        private static void OnTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int total) return;
            control.UpdateTotalUi(total);
            var viewModel = control.DataContext as PaginationViewModel;
            viewModel.Total = total;
        }
        #endregion
         
        #region Private Method
        private void UpdateShowTotalUi(bool showTotal)
        {
            TotalStackPanel.Visibility = showTotal ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTotalUi(int total)
        {
            TotalText.Text = total.ToString();
        }

        private void UpdatePageSizeUi(int pageSize)
        {
            SelectPageSize.SelectedIndex = pageSize / 10 - 1;
        }

        #endregion
    }
}
