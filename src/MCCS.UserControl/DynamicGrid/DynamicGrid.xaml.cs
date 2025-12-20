using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using MCCS.UserControl.DynamicGrid.FlattenedGrid;


namespace MCCS.UserControl.DynamicGrid
{
    /// <summary>
    /// DynamicGrid.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicGrid
    {
        private BinaryTreeManager? _binaryTreeManager;

        public DynamicGrid()
        {
            InitializeComponent(); 
        }

        private LayoutSettingModel? _layoutSettings;

        #region Private Method

        private void RefreshUi()
        {
            if (_binaryTreeManager == null) return;
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            // 设置行列定义
            (List<(GridUnitType, double)> rowDefs, List<(GridUnitType, double)> colDefs) = _binaryTreeManager.GetRowAndColumnDifitions();
            foreach ((GridUnitType type, var value) in rowDefs)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(value, type)
                });
            }
            foreach ((GridUnitType type, var value) in colDefs)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(value, type)
                });
            }
        }

        public void AddCellItem(LayoutNode cell, int row, int column)
        { 
        }

        private void OnSplitterDraged(object? sender, DragCompletedEventArgs e)
        {
            if (sender is not GridSplitter splitter) return;
            var grid = (Grid)splitter.Parent; 
            double sumLength = 0;
            // 行Splitter
            if (splitter.ResizeDirection == GridResizeDirection.Rows)
            {
                var row = Grid.GetRow(splitter);
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        for (var i = 0; i < row; i++)
                            sumLength += grid.RowDefinitions[i].ActualHeight;

                        var ratio = sumLength / grid.ActualHeight;
                        // 更新UI
                        MessageBox.Show($"更新后的比例: {ratio:P2}");
                    }));
            }
            else
            {
                var col = Grid.GetColumn(splitter);
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() =>
                    { 
                        for (var i = 0; i < col; i++)
                            sumLength += grid.ColumnDefinitions[i].ActualWidth;

                        var ratio = sumLength / grid.ActualWidth;
                        // 更新UI
                        MessageBox.Show($"更新后的比例: {ratio:P2}");
                    }));
                
            }
        }

        private void OnSplitRequested(object? sender, SplitRequestEventArgs e)
        {
            if (sender is not DependencyObject child || _layoutSettings == null || _binaryTreeManager == null) return;
            var contentId = Guid.NewGuid().ToString("N");
            var splitterId = Guid.NewGuid().ToString("N");
            var addNode = new CellItemControl
            {
                Tag = contentId
            }; 
            var gridSplitter = new GridSplitter
            {
                HorizontalAlignment = HorizontalAlignment.Stretch, 
                ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                Background = System.Windows.Media.Brushes.AliceBlue
            };
            gridSplitter.DragCompleted += OnSplitterDraged;
            if (e.Direction == CutDirectionEnum.Horizontal)
            {
                gridSplitter.ResizeDirection = GridResizeDirection.Rows;
                gridSplitter.Height = 2;
                _binaryTreeManager.CutHorizontal(e.CellId, contentId, splitterId);
            }
            else
            {
                gridSplitter.ResizeDirection = GridResizeDirection.Columns;
                gridSplitter.Width = 2;
                _binaryTreeManager.CutVertical(e.CellId, contentId, splitterId);
            } 
            _layoutSettings.Contents.Add(new UiContentElement(contentId)
            {
                CellType = CellTypeEnum.EditableElement,
                Content = addNode
            });
            _layoutSettings.Contents.Add(new UiContentElement(splitterId)
            {
                CellType = CellTypeEnum.SplitterElement,
                Content = gridSplitter
            });
            addNode.SplitRequested += OnSplitRequested;
            MainGrid.Children.Add(addNode);
            MainGrid.Children.Add(gridSplitter); 
            // 这里需要更新布局结构 
            var elementPositions = _binaryTreeManager.GetElementDisplacement(_layoutSettings.SpatialStructure);
            RefreshUi();
            foreach (var content in _layoutSettings.Contents)
            {
                if (!elementPositions.TryGetValue(content.Id, out (int row, int col, int rSpan, int cSpan) position)) return;
                Grid.SetRow(content.Content, position.row);
                Grid.SetColumn(content.Content, position.col);
                if (position.rSpan > 1)
                    Grid.SetRowSpan(content.Content, position.rSpan);
                if (position.cSpan > 1)
                    Grid.SetColumnSpan(content.Content, position.cSpan); 
            } 
        }

        #endregion

        /// <summary>
        /// 布局设置
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
            control._binaryTreeManager = new BinaryTreeManager(layoutSettings.SpatialStructure); 
            control.MainGrid.Children.Clear();
            control.RefreshUi();
            // 设置元素位置
            var elementPositions = control._binaryTreeManager.GetElementDisplacement(layoutSettings.SpatialStructure);
            foreach (var content in layoutSettings.Contents)
            {
                if (!elementPositions.TryGetValue(content.Id, out (int row, int col, int rSpan, int cSpan) position)) return; 
                Grid.SetRow(content.Content, position.row);
                Grid.SetColumn(content.Content, position.col);
                if (position.rSpan >= 1)
                    Grid.SetRowSpan(content.Content, position.rSpan);
                if (position.cSpan >= 1)
                    Grid.SetColumnSpan(content.Content, position.cSpan);
                if (content.Content is CellItemControl cellItemControl)
                {
                    cellItemControl.SplitRequested += control.OnSplitRequested;
                }

                if (content.Content is GridSplitter gridSplitter)
                {
                    gridSplitter.DragCompleted += control.OnSplitterDraged;
                }

                control.MainGrid.Children.Add(content.Content);
            }
        } 
    }
}
