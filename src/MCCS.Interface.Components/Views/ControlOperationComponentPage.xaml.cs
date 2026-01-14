using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views
{
    /// <summary>
    /// ControlOperationComponentPage.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "control-operation-component",
        "控制操作组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于控制通道操作和参数设置",
        Icon = "Cogs",
        Order = 1)]
    public partial class ControlOperationComponentPage
    {
        public ControlOperationComponentPage()
        {
            InitializeComponent();
        }
    }
}
