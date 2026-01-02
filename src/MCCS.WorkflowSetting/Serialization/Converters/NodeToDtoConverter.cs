using System.Windows.Media;

using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

namespace MCCS.WorkflowSetting.Serialization.Converters
{
    /// <summary>
    /// 节点到DTO转换器
    /// </summary>
    public static class NodeToDtoConverter
    {
        /// <summary>
        /// 将BaseNode转换为对应的DTO
        /// </summary>
        public static BaseNodeDto? Convert(BaseNode node) =>
            node switch
            {
                StepNode stepNode => ConvertStepNode(stepNode),
                BranchNode branchNode => ConvertBranchNode(branchNode),
                DecisionNode decisionNode => ConvertDecisionNode(decisionNode),
                StartNode startNode => ConvertStartNode(startNode),
                EndNode endNode => ConvertEndNode(endNode),
                // AddOpNode和TempPlaceholderAddNode不需要序列化，它们是UI辅助节点
                AddOpNode => null,
                TempPlaceholderAddNode => null,
                _ => ConvertBaseNode(node)
            };

        private static BaseNodeDto ConvertBaseNode(BaseNode node) =>
            new()
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = node.Type,
                Order = node.Order,
                Level = node.Level
            };

        private static BaseNodeDto ConvertStartNode(StartNode node) =>
            new()
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = NodeTypeEnum.Start,
                Order = node.Order,
                Level = node.Level
            };

        private static BaseNodeDto ConvertEndNode(EndNode node) =>
            new()
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = NodeTypeEnum.End,
                Order = node.Order,
                Level = node.Level
            };

        private static StepNodeDto ConvertStepNode(StepNode node) =>
            new()
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = NodeTypeEnum.Process,
                Order = node.Order,
                Level = node.Level,
                Title = node.Title,
                TitleBackgroundColor = GetColorString(node.TitleBackground)
            };

        private static BranchNodeDto ConvertBranchNode(BranchNode node) =>
            new()
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = NodeTypeEnum.Branch,
                Order = node.Order,
                Level = node.Level,
                Title = node.Title
            };

        private static DecisionNodeDto ConvertDecisionNode(DecisionNode node)
        {
            var dto = new DecisionNodeDto
            {
                Id = node.Id,
                Name = node.Name,
                Code = node.Code,
                Type = NodeTypeEnum.Decision,
                Order = node.Order,
                Level = node.Level,
                IsCollapse = node.IsCollapse,
                Branches = []
            };

            // 转换所有子分支
            foreach (var child in node.Children)
            {
                if (child is BranchStepListNodes branchStepList)
                {
                    dto.Branches.Add(ConvertBranchStepList(branchStepList));
                }
            }

            return dto;
        }

        /// <summary>
        /// 转换分支步骤列表
        /// </summary>
        public static BranchStepListDto ConvertBranchStepList(BranchStepListNodes branchStepList)
        {
            var dto = new BranchStepListDto
            {
                Id = branchStepList.Id,
                Name = branchStepList.Name,
                Code = branchStepList.Code,
                Type = NodeTypeEnum.BranchStepList,
                Order = branchStepList.Order,
                Level = branchStepList.Level,
                Nodes = []
            };

            // 转换分支中的所有节点
            foreach (var node in branchStepList.Nodes)
            {
                var nodeDto = Convert(node);
                if (nodeDto != null)
                {
                    dto.Nodes.Add(nodeDto);
                }
            }

            return dto;
        }

        /// <summary>
        /// 获取颜色的十六进制字符串表示
        /// </summary>
        private static string GetColorString(Brush? brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return "#FF9955"; // 默认颜色
        }
    }
}
