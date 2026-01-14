using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views
{
    /// <summary>
    /// ProjectDataMonitorComponentPage.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "data-monitor-component",
        "数据监控组件",
        InterfaceComponentCategory.Display,
        Description = "用于实时监控和显示测试数据",
        Icon = "MonitorDashboard",
        Order = 2)]
    public partial class ProjectDataMonitorComponentPage
    {
        public ProjectDataMonitorComponentPage()
        {
            InitializeComponent();
        }
    }
}
