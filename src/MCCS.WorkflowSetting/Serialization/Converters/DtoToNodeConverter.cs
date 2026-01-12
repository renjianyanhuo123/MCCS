using System.Windows.Media;

using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

namespace MCCS.WorkflowSetting.Serialization.Converters
{
    /// <summary>
    /// DTO到节点转换器
    /// </summary>
    public class DtoToNodeConverter(IEventAggregator eventAggregator, IDialogService dialogService)
    {
        /// <summary>
        /// 将DTO转换为对应的节点
        /// </summary>
        public BaseNode? Convert(BaseNodeDto dto)
        {
            switch(dto?.Type)
            {
                case NodeTypeEnum.Start:
                    return ConvertToStartNode(dto);
                case NodeTypeEnum.End:
                    return ConvertToEndNode(dto);
                case NodeTypeEnum.Process:
                    return ConvertToStepNode((StepNodeDto)dto);
                case NodeTypeEnum.Branch:
                    if (dto is BranchNodeDto dtoTemp)
                    {
                        return ConvertToBranchNode(dtoTemp);
                    } 
                    return null;
                case NodeTypeEnum.Decision:
                    return ConvertToDecisionNode((DecisionNodeDto)dto); 
            };
            return null;
        }

        private static StartNode ConvertToStartNode(BaseNodeDto dto) =>
            new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level
            };

        private static EndNode ConvertToEndNode(BaseNodeDto dto) =>
            new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Width = dto.Width,
                Height = dto.Height,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level
            };

        private StepNode ConvertToStepNode(StepNodeDto dto)
        {
            var node = new StepNode(dto.ParentId ?? string.Empty, eventAggregator)
            {
                Id = dto.Id,
                Width = dto.Width,
                Height = dto.Height,
                Name = dto.Name,
                Code = dto.Code,
                Order = dto.Order,
                Level = dto.Level,
                Title = dto.Title,
                TitleBackground = ParseColorBrush(dto.TitleBackgroundColor)
            };
            return node;
        }

        private BranchNode ConvertToBranchNode(BranchNodeDto dto) =>
            new(eventAggregator)
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Width = dto.Width,
                Height = dto.Height,
                Order = dto.Order,
                Level = dto.Level,
                Title = dto.Title
            };

        private DecisionNode ConvertToDecisionNode(DecisionNodeDto dto)
        { 
            var children = dto.Branches.Select(ConvertToBranchStepList).ToList();
            // 转换所有子分支
            var node = new DecisionNode(eventAggregator, dialogService, children)
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Width = dto.Width,
                Height = dto.Height,
                Order = dto.Order,
                Level = dto.Level,
                IsCollapse = dto.IsCollapse
            };
            return node;
        }

        /// <summary>
        /// 转换分支步骤列表
        /// </summary>
        public BranchStepListNodes ConvertToBranchStepList(BranchStepListDto dto)
        {
            var nodes = dto.Nodes.Select(Convert).OfType<BaseNode>().ToList();
            // 转换分支中的所有节点
            var branchStepList = new BranchStepListNodes(eventAggregator, dialogService, nodes)
            {
                Id = dto.Id,
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
