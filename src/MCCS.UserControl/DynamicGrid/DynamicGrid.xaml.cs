using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MCCS.UserControl.DynamicGrid
{
    /// <summary>
    /// DynamicGrid.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicGrid
    {
        public DynamicGrid()
        {
            InitializeComponent();
        }

        private LayoutSettingModel? _layoutSettings;

        #region Private Method
        public void AddCellItem(CellViewModel cell, int row, int column)
        {
            cell.Content.Tag = cell.Id;
            // 设置 Grid 附加属性
            Grid.SetRow(cell.Content, cell.Row);
            Grid.SetColumn(cell.Content, cell.Column);
            if (cell.RowSpan > 1)
                Grid.SetRowSpan(cell.Content, cell.RowSpan); 
            if (cell.ColumnSpan > 1)
                Grid.SetColumnSpan(cell.Content, cell.ColumnSpan);
            if (cell is { CellType: CellTypeEnum.EditableElement, Content: CellItemControl cellItemControl })
            {
                cellItemControl.SplitRequested += OnSplitRequested;
            } 
            MainGrid.Children.Add(cell.Content);
        }

        private void OnSplitRequested(object? sender, SplitRequestEventArgs e)
        {
            if (sender is not DependencyObject child && _layoutSettings == null)
                return;
            // 到这里，DynamicGrid 已经完全知道：
            // ✔ 哪个子元素
            // ✔ 哪个 Row / Column
            // ✔ 横切还是竖切
            /*
             * <GridSplitter
               Grid.Column="3"
               Width="5"
               ResizeDirection="Columns"
               ResizeBehavior="PreviousAndNext"
               Background="Transparent"/>
             */
            var cellItem = new CellItemControl()
            {
                Width = 400,
                Height = 200
            };
            if (e.Direction == CutDirectionEnum.Horizontal)
            {
                // 横切;只改变行
                var gridSplitter = new GridSplitter()
                {
                    Height = 5,
                    Width = double.NaN,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    ResizeDirection = GridResizeDirection.Rows,
                    ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                    Background = System.Windows.Media.Brushes.AliceBlue
                };
                _layoutSettings?.Cells.Add(new CellViewModel
                {
                    CellType = CellTypeEnum.EditableElement,
                    Content = cellItem
                });
                _layoutSettings?.Cells.Add(new CellViewModel
                {
                    CellType = CellTypeEnum.SplitterElement,
                    Content = gridSplitter
                });
                MainGrid.Children.Add(gridSplitter);
                MainGrid.Children.Add(cellItem);
            }
            else
            {
                // 竖切;只改变列
                var gridSplitter = new GridSplitter()
                {
                    Height = double.NaN,
                    Width = 5,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    ResizeDirection = GridResizeDirection.Columns,
                    ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                    Background = System.Windows.Media.Brushes.AliceBlue
                };
            }
        }

        #endregion

        /// <summary>
        /// 布局设置, JSON格式
        /// </summary>
        public LayoutSettingModel LayoutSettings
        {
            get => (LayoutSettingModel)GetValue(LayoutSettingsProperty);
            set => SetValue(LayoutSettingsProperty, value);
        }

        public static readonly DependencyProperty LayoutSettingsProperty =
            DependencyProperty.Register(
                nameof(LayoutSettings),
                typeof(LayoutSettingModel),
                typeof(DynamicGrid),
                new PropertyMetadata(null, OnLayoutSettingsChanged));

        /// <summary>
        /// 切分单元格命令
        /// </summary>
        public ICommand CutCommand
        {
            get => (ICommand)GetValue(CutCommandProperty);
            set => SetValue(CutCommandProperty, value);
        }

        public static readonly DependencyProperty CutCommandProperty =
            DependencyProperty.Register(
                nameof(CutCommand),
                typeof(ICommand),
                typeof(DynamicGrid),
                new PropertyMetadata(null));

        private static void OnLayoutSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DynamicGrid control) return;
            if (e.NewValue is not LayoutSettingModel layoutSettings) return;
            control._layoutSettings = layoutSettings;
            control.MainGrid.RowDefinitions.Clear();
            control.MainGrid.ColumnDefinitions.Clear();
            control.MainGrid.Children.Clear();
            foreach (var row in layoutSettings.Rows)
            {
                control.MainGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(row.Value, row.UnitType)
                });
            }
            foreach (var column in layoutSettings.Columns)
            {
                control.MainGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(column.Value, column.UnitType)
                });
            }
            foreach (var cell in layoutSettings.Cells)
            {
                control.AddCellItem(cell, cell.Row, cell.Column);
            }
        } 
    }
}
