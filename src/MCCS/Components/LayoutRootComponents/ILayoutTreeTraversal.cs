using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;

namespace MCCS.Components.LayoutRootComponents
{
    public interface ILayoutTreeTraversal
    {
        List<BaseNode> PostOrderToBaseNodes(LayoutNode root);

        LayoutNode BuildRootNode(CellTypeEnum cellType, List<BaseNode> nodes, List<MethodUiComponentsModel> components);
    }
}
