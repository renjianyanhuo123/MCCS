using MCCS.ViewModels.ProjectManager.Components;

namespace MCCS.Components.LayoutRootComponents
{
    /// <summary>
    /// CellContainerComponent.xaml 的交互逻辑
    /// </summary>
    public partial class CellContainerComponent
    {
        public CellContainerComponent()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                var viewModel = DataContext as CellContainerComponentViewModel ?? throw new ArgumentException(nameof(CellContainerComponentViewModel));
                RegionManager.SetRegionManager(ComponentElementRegion, viewModel.ScopedRegionManager);
                RegionManager.SetRegionName(ComponentElementRegion, "ProjectContentRegion");
                viewModel.ScopedRegionManager.RequestNavigate("ProjectContentRegion", new Uri(nameof(ProjectChartComponentPageViewModel), UriKind.Relative));
            };
            
        }
    }
}
