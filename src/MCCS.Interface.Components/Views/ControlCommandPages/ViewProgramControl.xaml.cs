using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views.ControlCommandPages
{
    /// <summary>
    /// ViewProgramControl.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "program-control-component",
        "程序控制组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于程序控制模式的参数设置和操作",
        Icon = "CodeBraces",
        Order = 12)]
    public partial class ViewProgramControl
    {
        public ViewProgramControl()
        {
            InitializeComponent();
        }
    }
}
