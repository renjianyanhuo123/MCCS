using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Services.ProjectServices;

namespace MCCS.Components.LayoutRootComponents
{
    public class LayoutTreeTraversal : ILayoutTreeTraversal
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMethodRepository _methodRepository; 
        private readonly IDialogService _dialogService; 
        private readonly IProjectComponentFactoryService _projectComponentFactoryService;

        public LayoutTreeTraversal(
            IEventAggregator eventAggregator,
            IProjectComponentFactoryService projectComponentFactoryService,
            IDialogService dialogService,
            IMethodRepository methodRepository)
        {
            _projectComponentFactoryService = projectComponentFactoryService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            _methodRepository = methodRepository;
        }

        /// <summary>
        /// 后序遍历布局树，转换为BaseNode列表
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public List<BaseNode> PostOrderToBaseNodes(LayoutNode root)
        {
            if (root == null)
                return [];

            var result = new List<BaseNode>(128); // 可按规模调整
            var stack = new Stack<LayoutNode>();
            LayoutNode? current = root;
            LayoutNode? lastVisited = null;

            while (current != null || stack.Count > 0)
            {
                // 一直向左下
                while (current != null)
                {
                    stack.Push(current);
                    current = current.LeftNode;
                } 
                var peek = stack.Peek(); 
                // 右子树未访问，转向右
                if (peek.RightNode != null && lastVisited != peek.RightNode)
                {
                    current = peek.RightNode;
                }
                else
                {
                    // 左右子树都处理完，访问当前节点
                    stack.Pop(); 
                    result.Add(CreateBaseNode(peek)); 
                    lastVisited = peek;
                }
            }
            return result;
        } 

        public LayoutNode BuildRootNode(CellTypeEnum cellType, List<BaseNode> nodes, List<MethodUiComponentsModel> components)
        {
            if (nodes.Count == 0) return CreateLayoutNode(cellType);
            LayoutNode? loopNode = null;
            var nodeDic = new Dictionary<string, LayoutNode>();
            //(1)构建二叉树;nodes采用后序遍历存储
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case CellNode cellNode:
                        loopNode = CreateLayoutNode(cellType, cellNode, components.FirstOrDefault(c => c.Id == cellNode.NodeId));
                        break;
                    case SplitterNode splitterNode:
                        if (splitterNode.LeftNodeId == null
                            || !nodeDic.ContainsKey(splitterNode.LeftNodeId)
                            || splitterNode.RightNodeId == null
                            || !nodeDic.ContainsKey(splitterNode.RightNodeId))
                            throw new InvalidOperationException("节点数据异常，无法构建布局树");
                        var leftNode = nodeDic[splitterNode.LeftNodeId];
                        var rightNode = nodeDic[splitterNode.RightNodeId];
                        if (splitterNode.NodeType == NodeTypeEnum.SplitterHorizontal)
                        {
                            loopNode = new SplitterHorizontalLayoutNode(splitterNode.LeftRatio,
                                splitterNode.RightRatio, splitterNode.SplitterSize, leftNode, rightNode);
                        }
                        else
                        {
                            loopNode = new SplitterVerticalLayoutNode(splitterNode.LeftRatio,
                                splitterNode.RightRatio, splitterNode.SplitterSize, leftNode, rightNode);
                        }
                        break;
                    default:
                        throw new NotSupportedException("不支持该种类型的节点");
                }
                nodeDic.Add(node.Id, loopNode);
            }
            if (loopNode == null) throw new InvalidOperationException("节点数据异常，无法构建布局树");
            return loopNode;
        }

        private LayoutNode CreateLayoutNode(CellTypeEnum cellType, CellNode? node = null, MethodUiComponentsModel? component = null)
        {
            if (cellType == CellTypeEnum.DisplayOnly)
            {
                return new CellContainerComponentViewModel(_dialogService, _projectComponentFactoryService, _eventAggregator, node);
            }

            if (node == null || component == null)
            {
                return new CellEditableComponentViewModel(_eventAggregator, _methodRepository);
            }

            return new CellEditableComponentViewModel(_eventAggregator, _methodRepository, node, component);
        }

        private static BaseNode CreateBaseNode(LayoutNode node) =>
            node switch
            {
                SplitterHorizontalLayoutNode splitterHorizontalLayoutNode => new SplitterNode(id: node.Id,
                    type: NodeTypeEnum.SplitterHorizontal, leftRatio: splitterHorizontalLayoutNode.LeftRatio.Value,
                    rightRatio: splitterHorizontalLayoutNode.RightRatio.Value,
                    splitterSize: splitterHorizontalLayoutNode.SplitterSize.Value, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                SplitterVerticalLayoutNode splitterVerticalLayoutNode => new SplitterNode(id: node.Id,
                    type: NodeTypeEnum.SplitterVertical, leftRatio: splitterVerticalLayoutNode.LeftRatio.Value,
                    rightRatio: splitterVerticalLayoutNode.RightRatio.Value,
                    splitterSize: splitterVerticalLayoutNode.SplitterSize.Value, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                CellEditableComponentViewModel cellEditableComponentViewModel => new CellNode(id: node.Id,
                    type: NodeTypeEnum.Cell, nodeId: cellEditableComponentViewModel.NodeId,
                    paramterJson: cellEditableComponentViewModel.ParamterJson, parentId: node.Parent?.Id,
                    leftNodeId: node.LeftNode?.Id, rightNodeId: node.RightNode?.Id),
                _ => throw new InvalidOperationException("未知布局节点类型，无法转换为BaseNode")
            };
    }

}
