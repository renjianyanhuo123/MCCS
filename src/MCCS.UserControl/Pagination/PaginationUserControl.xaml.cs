using MCCS.UserControl.Params;
using System.Windows;
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
            Loaded += PaginationUserControl_Loaded;
        }

        #region 回调Command
        public static readonly DependencyProperty PageChangedCommandProperty =
            DependencyProperty.Register(
                nameof(PageChangedCommand), 
                typeof(ICommand), 
                typeof(PaginationUserControl), 
                new PropertyMetadata(null, OnPageChangedCommandChanged));

        public ICommand PageChangedCommand
        {
            get => (ICommand)GetValue(PageChangedCommandProperty);
            set => SetValue(PageChangedCommandProperty, value);
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
        private static void OnPageChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control || e.NewValue is not ICommand pageChangedCommand) return;
            // 重新绑定 ViewModel 的回调
            control._viewModel.PageChanged -= control.BindingPageChanged;
            control._viewModel.PageChanged += control.BindingPageChanged;
        }

        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int pageSize) return;
            if (control.DataContext is PaginationViewModel viewModel)
            {
                viewModel.PageSize = pageSize;
            }
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int currentPage) return; 
            if (control.DataContext is PaginationViewModel viewModel)
            {
                viewModel.CurrentPage = currentPage;
            }
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
            if (control.DataContext is PaginationViewModel viewModel)
            {
                viewModel.CurrentPage = defaultCurrentPage;
            }
        }

        private static void OnDefaultPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int defaultPageSize) return;
            control.UpdatePageSizeUi(defaultPageSize);
            if (control.DataContext is PaginationViewModel viewModel)
            {
                viewModel.PageSize = defaultPageSize;
            }
        }

        private static void OnTotalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PaginationUserControl control) return;
            if (e.NewValue is not int total) return;
            control.UpdateTotalUi(total); 
            if (control.DataContext is PaginationViewModel viewModel)
            {
                viewModel.Total = total;
            }
        }
        #endregion
         
        #region Private Method
        private void BindingPageChanged(PageChangedParam param)
        {
            if (PageChangedCommand != null && PageChangedCommand.CanExecute(param))
            {
                PageChangedCommand.Execute(param);
            }
        }

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
