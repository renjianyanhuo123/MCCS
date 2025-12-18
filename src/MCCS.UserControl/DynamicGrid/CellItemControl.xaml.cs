using System.Windows;

namespace MCCS.UserControl.DynamicGrid
{
    /// <summary>
    /// CellItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class CellItemControl
    {
        public CellItemControl()
        {
            InitializeComponent();
        }

        public event EventHandler<SplitRequestEventArgs>? SplitRequested;

        private void OnHorizontalCutClick(object sender, RoutedEventArgs e) =>
            SplitRequested?.Invoke(
                this,
                new SplitRequestEventArgs
                {
                    CellId = Tag.ToString() ?? "",
                    Direction = CutDirectionEnum.Horizontal
                }
            );

        private void OnVerticalCutClick(object sender, RoutedEventArgs e) =>
            SplitRequested?.Invoke(
                this,
                new SplitRequestEventArgs
                {
                    CellId = Tag.ToString() ?? "",
                    Direction = CutDirectionEnum.Vertical
                }
            );
    }
}
