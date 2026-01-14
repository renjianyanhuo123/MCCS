using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views.ControlCommandPages
{
    /// <summary>
    /// ViewStaticControl.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "static-control-component",
        "静态控制组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于静态试验控制模式的参数设置和操作",
        Icon = "Target",
        Order = 11)]
    public partial class ViewStaticControl
    {
        public ViewStaticControl()
        {
            InitializeComponent();
        }
    }
}
