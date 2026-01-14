using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views
{
    /// <summary>
    /// ProjectChartComponentPage.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "chart-component",
        "曲线图表组件",
        InterfaceComponentCategory.Display,
        Description = "用于显示实时数据曲线图表",
        Icon = "ChartLine",
        Order = 1)]
    public partial class ProjectChartComponentPage
    {
        public ProjectChartComponentPage()
        {
            InitializeComponent();
        }
    }
}
