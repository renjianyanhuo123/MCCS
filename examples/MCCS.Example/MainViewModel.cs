using System.Windows;
using System.Windows.Controls;

using MCCS.UserControl;
using MCCS.UserControl.DynamicGrid;
using MCCS.UserControl.DynamicGrid.FlattenedGrid;

namespace MCCS.Example
{
    public class MainViewModel : BindingBase
    {
        // private object _param;
        //private int _total;
        public MainViewModel()
        {
            var node1 = new CellLayoutNode();
            var node2 = new CellLayoutNode();
            var node3 = new CellLayoutNode();
            var node4 = new CellLayoutNode();
            var leftSplitterNode = new SplitterNode(CutDirectionEnum.Horizontal, node1, node2)
            {
                Ratio = 0.3
            };
            var rightSplitterNode = new SplitterNode(CutDirectionEnum.Horizontal, node3, node4)
            {
                Ratio = 0.7
            };
            var root = new SplitterNode(CutDirectionEnum.Vertical, leftSplitterNode, rightSplitterNode)
            {
                Ratio = 0.5
            }; 
            LayoutSetting = new LayoutSettingModel
            { 
                Contents =
                [
                    new UiContentElement(node1.Id)
                    {
                        Content = new CellItemControl
                        {
                            Tag = node1.Id
                        },
                    },

                    new UiContentElement(node2.Id)
                    {
                        Content = new CellItemControl
                        {
                            Tag = node2.Id
                        },
                    },

                    new UiContentElement(node3.Id)
                    {
                        Content = new CellItemControl
                        {
                            Tag = node3.Id
                        },
                    },

                    new UiContentElement(node4.Id)
                    {
                        Content = new CellItemControl
                        {
                            Tag = node4.Id
                        },
                    },
                    new UiContentElement(leftSplitterNode.Id)
                    {
                        CellType = CellTypeEnum.SplitterElement,
                        Content =new GridSplitter()
                        {
                            Height = 5,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            ResizeDirection = GridResizeDirection.Rows,
                            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                            Background = System.Windows.Media.Brushes.OrangeRed
                        }
                    },
                    new UiContentElement(rightSplitterNode.Id)
                    {
                        CellType = CellTypeEnum.SplitterElement,
                        Content = new GridSplitter
                        { 
                            Height = 5,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            ResizeDirection = GridResizeDirection.Rows,
                            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                            Background = System.Windows.Media.Brushes.OrangeRed
                        }
                    },
                    new UiContentElement(root.Id)
                    {
                        Content = new GridSplitter
                        { 
                            Width = 5,
                            Height = double.NaN,
                            HorizontalAlignment = HorizontalAlignment.Stretch, 
                            ResizeDirection = GridResizeDirection.Columns,
                            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                            Background = System.Windows.Media.Brushes.OrangeRed
                        }
                    }
                ], SpatialStructure = root
            };
        }

        //public int Total
        //{
        //    get => _total;
        //    set => SetProperty(ref _total, value);
        //}
        

        //public ICommand OnPageChanged => new RelayCommand(param =>
        //{
        //    var p = param as PageChangedParam;
        //    Thread.Sleep(1000);
        //    Debug.WriteLine($"当前页:{p?.CurrentPage},PageSize:{p?.PageSize}");
        //}, _ => true);

        //private string _sourceName = "123123123";

        //public string SourceName
        //{
        //    get => _sourceName;
        //    set => SetProperty(ref _sourceName, value);
        //}

        //private string _targetName = "2132131"; 
        //public string TargetName
        //{
        //    get => _targetName;
        //    set => SetProperty(ref _targetName, value);
        //}

        //public ObservableCollection<TransferItemModel> SourceList { get; } = [
        //    new()
        //    {
        //        Id = Guid.NewGuid().ToString("N"),
        //        Name = "12333",
        //        IsSelected = true
        //    },
        //    new()
        //    {
        //        Id = Guid.NewGuid().ToString("N"),
        //        Name = "HHHH",
        //        IsSelected = true
        //    }
        //];

        //public ObservableCollection<TransferItemModel> TargetList { get; } = [];

        private LayoutSettingModel _layoutSettingModel;
        public LayoutSettingModel LayoutSetting { get => _layoutSettingModel; set => SetProperty(ref _layoutSettingModel, value); }

    }
}
