using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.Views.ControlCommandPages
{
    /// <summary>
    /// ViewFatigueControl.xaml 的交互逻辑
    /// </summary>
    [InterfaceComponent(
        "fatigue-control-component",
        "疲劳控制组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于疲劳试验控制模式的参数设置和操作",
        Icon = "Repeat",
        Order = 13)]
    public partial class ViewFatigueControl
    {
        public ViewFatigueControl()
        {
            InitializeComponent();
        }
    }
}
