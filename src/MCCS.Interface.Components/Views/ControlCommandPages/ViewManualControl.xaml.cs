using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views.ControlCommandPages
{
    /// <summary>
    /// ViewManualControl.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "manual-control-component",
        "手动控制组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于手动控制模式的参数设置和操作",
        Icon = "HandBackRight",
        Order = 10)]
    public partial class ViewManualControl
    {
        public ViewManualControl()
        {
            InitializeComponent();
        }
    }
}
