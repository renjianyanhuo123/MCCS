using System.Windows.Media;

using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

namespace MCCS.WorkflowSetting.Serialization.Converters
{
    /// <summary>
    /// DTO到节点转换器
    /// </summary>
    public class DtoToNodeConverter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;

        public DtoToNodeConverter(IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
        }

        /// <summary>
        /// 将DTO转换为对应的节点
        /// </summary>
        public BaseNode? Convert(BaseNodeDto dto, BaseNode? parent) =>
            dto.Type switch
            {
                NodeTypeEnum.Start => ConvertToStartNode(dto, parent),
                NodeTypeEnum.End => ConvertToEndNode(dto, parent),
                NodeTypeEnum.Process => ConvertToStepNode((StepNodeDto)dto, parent),
                NodeTypeEnum.Branch => ConvertToBranchNode((BranchNodeDto)dto, parent),
                NodeTypeEnum.Decision => ConvertToDecisionNode((DecisionNodeDto)dto, parent),
                _ => null
            };

        private StartNode ConvertToStartNode(BaseNodeDto dto, BaseNode? parent) =>
            new(parent)
            {
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level
            };

        private EndNode ConvertToEndNode(BaseNodeDto dto, BaseNode? parent) =>
            new(parent)
            {
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level
            };

        private StepNode ConvertToStepNode(StepNodeDto dto, BaseNode? parent)
        {
            var node = new StepNode(parent?.Id ?? string.Empty, _eventAggregator)
            {
                Parent = parent,
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level,
                Title = dto.Title,
                TitleBackground = ParseColorBrush(dto.TitleBackgroundColor)
            };
            return node;
        }

        private BranchNode ConvertToBranchNode(BranchNodeDto dto, BaseNode? parent) =>
            new(_eventAggregator, parent)
            {
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level,
                Title = dto.Title
            };

        private DecisionNode ConvertToDecisionNode(DecisionNodeDto dto, BaseNode? parent)
        {
            var children = new List<BranchStepListNodes>();
            // 转换所有子分支
            foreach (var branchDto in dto.Branches)
            {
                var branchStepList = ConvertToBranchStepList(branchDto, null);
                children.Add(branchStepList);
            }
            var node = new DecisionNode(_eventAggregator, _dialogService, children)
            {
                Parent = parent,
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level,
                IsCollapse = dto.IsCollapse
            };   
            return node;
        }

        /// <summary>
        /// 转换分支步骤列表
        /// </summary>
        public BranchStepListNodes ConvertToBranchStepList(BranchStepListDto dto, BaseNode? parent)
        {
            var nodes = new List<BaseNode>();
            // 转换分支中的所有节点
            foreach (var nodeDto in dto.Nodes)
            {
                var node = Convert(nodeDto, null);
                if (node != null)
                {
                    nodes.Add(node);
                }
            }
            var branchStepList = new BranchStepListNodes(_eventAggregator, nodes)
            {
                Parent = parent,
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level
            };
            // 在每个业务节点后面添加AddOpNode
            InsertAddOpNodes(branchStepList); 
            return branchStepList;
        }

        /// <summary>
        /// 在业务节点之后插入AddOpNode
        /// </summary>
        private void InsertAddOpNodes(BoxListNodes listNodes)
        {
            var nodesToInsert = new List<(int index, AddOpNode node)>();

            for (var i = 0; i < listNodes.Nodes.Count; i++)
            {
                var currentNode = listNodes.Nodes[i];
                // 在StepNode、DecisionNode、BranchNode后添加AddOpNode
                if (currentNode is StepNode or DecisionNode or BranchNode)
                {
                    nodesToInsert.Add((i + 1, new AddOpNode(listNodes)));
                }
            }

            // 从后向前插入，避免索引问题
            for (var i = nodesToInsert.Count - 1; i >= 0; i--)
            {
                (var index, AddOpNode node) = nodesToInsert[i];
                if (index <= listNodes.Nodes.Count)
                {
                    listNodes.Nodes.Insert(index, node);
                }
            }
        }

        /// <summary>
        /// 解析颜色字符串为Brush
        /// </summary>
        private static SolidColorBrush ParseColorBrush(string colorString)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                return new SolidColorBrush(color);
            }
            catch
            {
                // 默认颜色
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"));
            }
        }
    }
}
