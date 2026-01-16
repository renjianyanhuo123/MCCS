using System.Collections.ObjectModel;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters;

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
    public class ControlOperationComponentPageViewModel : BaseComponentViewModel
    {
        public ControlOperationComponentPageViewModel(List<ControlOperationParamModel> paramModels)
        {
            CombineCommand = new DelegateCommand(ExecuteCombineCommand);
            foreach (var controlChannel in paramModels)
            {
                ControlUnits.Add(new ControlSingleUnitComponent
                {
                    Title = controlChannel.ControlChannelName,
                    ControlUnitId = Guid.NewGuid().ToString("N"),
                    ChildComponent = new ControlCombineUnitChildComponent(controlChannel.ControlChannelId, controlChannel.ControlChannelName, controlChannel.AllowedControlModes)
                });
            }
        }

        #region Property 
        public ObservableCollection<ControlUnitComponent> ControlUnits { get; } = [];
        #endregion

        #region Command
        public DelegateCommand CombineCommand { get; }
        #endregion

        #region Private Method

        private void OnUnLockEvent(ControlCombineUnitComponent sender, List<ControlCombineUnitChildComponent> children)
        {
            ControlUnits.Remove(sender);
            foreach (var child in children)
            {
                ControlUnits.Add(new ControlSingleUnitComponent
                {
                    Title = child.ChannelName,
                    ControlUnitId = Guid.NewGuid().ToString("N"),
                    ChildComponent = child
                });
            }
        }
        private void ExecuteCombineCommand()
        {
            var count = 0;
            var tempComponents = new List<ControlSingleUnitComponent>();
            foreach (var controlUnit in ControlUnits)
            {
                if (controlUnit is ControlSingleUnitComponent { IsParticipateCombineControl: true } singleUnitComponent)
                {
                    tempComponents.Add(singleUnitComponent);
                    count++;
                }
            }
            if (count <= 1) return;
            foreach (var item in tempComponents)
            {
                ControlUnits.Remove(item);
            }
            var children = tempComponents.Select(s => new ControlCombineUnitChildComponent(
                s.ChildComponent.ChannelId,
                s.ChildComponent.ChannelName,
                s.ChildComponent.ControlModeSelections
                    .Select(c => (ControlModeTypeEnum)c.ControlModeId))).ToList();
            var combineComponent = new ControlCombineUnitComponent(children);
            combineComponent.UnLockEvent += OnUnLockEvent;
            ControlUnits.Add(combineComponent);
        }
        #endregion
    }
}
