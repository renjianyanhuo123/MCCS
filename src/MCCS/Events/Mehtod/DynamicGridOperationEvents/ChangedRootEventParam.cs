using MCCS.Components.LayoutRootComponents.ViewModels;

namespace MCCS.Events.Mehtod.DynamicGridOperationEvents
{
    public record ChangedRootEventParam
    {
        public required LayoutNode Root { get; init; }
    }
}
