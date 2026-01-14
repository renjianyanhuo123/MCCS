using System.Collections.ObjectModel;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    /// <summary>
    /// 控制操作组件
    /// </summary>
    [InterfaceComponent(
        "control-operation-component",
        "控制操作组件",
        InterfaceComponentCategory.Interaction,
        Description = "用于控制通道操作和参数设置",
        Icon = "Cogs",
        Order = 1)]
    public class ControlOperationComponentPageViewModel : BindableBase
    {
        public ControlOperationComponentPageViewModel()
        {
        }

        #region Property 
        public ObservableCollection<ControlUnitComponent> ControlUnits { get; } = []; 
        #endregion
    }
}
