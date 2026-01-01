using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using MCCS.WorkflowSetting.Models.Edges;
using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

namespace MCCS.WorkflowSetting.Serialization
{
    /// <summary>
    /// 工作流序列化服务实现
    /// </summary>
    public class WorkflowSerializer : IWorkflowSerializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        #region 序列化

        public string SerializeToJson(StepListNodes rootNode, string workflowName = "")
        {
            var workflowDto = SerializeToDto(rootNode, workflowName);
            return JsonSerializer.Serialize(workflowDto, _jsonOptions);
        }

        public WorkflowDto SerializeToDto(StepListNodes rootNode, string workflowName = "")
        {
            if (rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            var workflowDto = new WorkflowDto
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = workflowName,
                Version = "1.0.0",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 序列化根节点
            workflowDto.RootNode = ConvertNodeToDto(rootNode);

            // 收集所有连接线
            workflowDto.Connections = CollectAllConnections(rootNode);

            return workflowDto;
        }

        private NodeDto ConvertNodeToDto(BaseNode node)
        {
            var nodeDto = new NodeDto
            {
                Id = node.Id,
                Type = node.Type,
                Name = node.Name,
                Code = node.Code,
                Index = node.Index,
                Order = node.Order,
                Level = node.Level,
                PositionX = node.Position.X,
                PositionY = node.Position.Y,
                Width = node.Width,
                Height = node.Height
            };

            // 根据节点类型序列化特定属性
            switch (node)
            {
                case StepNode stepNode:
                    SerializeStepNode(stepNode, nodeDto);
                    break;

                case DecisionNode decisionNode:
                    SerializeDecisionNode(decisionNode, nodeDto);
                    break;

                case BoxListNodes boxListNodes:
                    SerializeBoxListNodes(boxListNodes, nodeDto);
                    break;

                case BranchNode branchNode:
                    SerializeBranchNode(branchNode, nodeDto);
                    break;
            }

            return nodeDto;
        }

        private void SerializeStepNode(StepNode stepNode, NodeDto nodeDto)
        {
            nodeDto.Properties["Title"] = stepNode.Title;

            // 将Brush转换为颜色字符串
            if (stepNode.TitleBackground is SolidColorBrush brush)
            {
                nodeDto.Properties["TitleBackgroundColor"] = brush.Color.ToString();
            }
        }

        private void SerializeDecisionNode(DecisionNode decisionNode, NodeDto nodeDto)
        {
            nodeDto.Properties["IsCollapse"] = decisionNode.IsCollapse;
            nodeDto.Properties["DecisionNum"] = decisionNode.DecisionNum;
            nodeDto.Properties["ItemSpacing"] = decisionNode.ItemSpacing;
            nodeDto.Properties["BorderWidth"] = decisionNode.BorderWidth;
            nodeDto.Properties["BorderHeight"] = decisionNode.BorderHeight;

            // 序列化子分支节点
            foreach (var child in decisionNode.Children)
            {
                nodeDto.Children.Add(ConvertNodeToDto(child));
            }
        }

        private void SerializeBoxListNodes(BoxListNodes boxListNodes, NodeDto nodeDto)
        {
            nodeDto.Properties["AddActionDistance"] = boxListNodes.AddActionDistance;

            // 序列化子节点列表（排除AddOpNode和TempPlaceholderAddNode）
            foreach (var child in boxListNodes.Nodes)
            {
                // 跳过临时节点和添加操作节点
                if (child.Type == NodeTypeEnum.Action || child.Type == NodeTypeEnum.TempPlaceholder)
                    continue;

                nodeDto.Children.Add(ConvertNodeToDto(child));
            }
        }

        private void SerializeBranchNode(BranchNode branchNode, NodeDto nodeDto)
        {
            nodeDto.Properties["Title"] = branchNode.Title;
        }

        private List<ConnectionDto> CollectAllConnections(BaseNode rootNode)
        {
            var connections = new List<ConnectionDto>();
            var visited = new HashSet<string>();

            CollectConnectionsRecursive(rootNode, connections, visited);

            return connections;
        }

        private void CollectConnectionsRecursive(BaseNode node, List<ConnectionDto> connections, HashSet<string> visited)
        {
            if (!visited.Add(node.Id))
                return;

            if (node is BoxListNodes boxListNodes)
            {
                // 收集当前容器的连接线
                foreach (var connection in boxListNodes.Connections)
                {
                    connections.Add(new ConnectionDto
                    {
                        Id = connection.Id,
                        Type = connection.Type,
                        Points = connection.Points.Select(p => new PointDto(p.X, p.Y)).ToList()
                    });
                }

                // 递归处理子节点
                foreach (var child in boxListNodes.Nodes)
                {
                    CollectConnectionsRecursive(child, connections, visited);
                }
            }
            else if (node is DecisionNode decisionNode)
            {
                // 递归处理决策节点的子分支
                foreach (var child in decisionNode.Children)
                {
                    CollectConnectionsRecursive(child, connections, visited);
                }
            }
        }

        #endregion

        #region 反序列化

        public StepListNodes DeserializeFromJson(string json, IEventAggregator eventAggregator, IDialogService? dialogService = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON字符串不能为空", nameof(json));

            var workflowDto = JsonSerializer.Deserialize<WorkflowDto>(json, _jsonOptions);
            if (workflowDto == null)
                throw new InvalidOperationException("JSON反序列化失败");

            return DeserializeFromDto(workflowDto, eventAggregator, dialogService);
        }

        public StepListNodes DeserializeFromDto(WorkflowDto workflowDto, IEventAggregator eventAggregator, IDialogService? dialogService = null)
        {
            if (workflowDto?.RootNode == null)
                throw new ArgumentException("工作流DTO或根节点不能为空", nameof(workflowDto));

            // 创建根节点
            var rootNode = new StepListNodes(eventAggregator);

            // 清空默认节点
            rootNode.Nodes.Clear();
            rootNode.Connections.Clear();

            // 重建节点树
            RestoreNodeFromDto(workflowDto.RootNode, rootNode, eventAggregator, dialogService);

            // 触发渲染更新
            rootNode.RenderChanged();

            return rootNode;
        }

        private void RestoreNodeFromDto(NodeDto nodeDto, BaseNode parentNode, IEventAggregator eventAggregator, IDialogService? dialogService)
        {
            if (nodeDto.Type == NodeTypeEnum.StepList || nodeDto.Type == NodeTypeEnum.BranchStepList)
            {
                // 对于容器类型节点，恢复其子节点
                if (parentNode is BoxListNodes boxListNodes)
                {
                    // 恢复基础属性
                    RestoreBaseNodeProperties(nodeDto, boxListNodes);

                    // 恢复子节点
                    foreach (var childDto in nodeDto.Children)
                    {
                        var childNode = CreateNodeFromDto(childDto, boxListNodes, eventAggregator, dialogService);
                        if (childNode != null)
                        {
                            childNode.Parent = boxListNodes;
                            boxListNodes.Nodes.Add(childNode);

                            // 递归恢复子节点的子节点
                            if (childDto.Children.Count > 0)
                            {
                                RestoreNodeFromDto(childDto, childNode, eventAggregator, dialogService);
                            }
                        }
                    }

                    // 添加AddOpNode（但不添加到最后，因为已经在子节点中了）
                    // 检查最后一个节点是否是EndNode，如果是，在倒数第二个位置添加AddOpNode
                    if (boxListNodes.Nodes.Count > 0 && boxListNodes.Nodes[^1].Type == NodeTypeEnum.End)
                    {
                        boxListNodes.Nodes.Insert(boxListNodes.Nodes.Count - 1, new AddOpNode(boxListNodes));
                    }
                    else
                    {
                        boxListNodes.Nodes.Add(new AddOpNode(boxListNodes));
                    }
                }
            }
        }

        private BaseNode? CreateNodeFromDto(NodeDto nodeDto, BaseNode? parentNode, IEventAggregator eventAggregator, IDialogService? dialogService)
        {
            // 获取父节点的Id，用于StepNode和BranchNode的构造函数
            var parentId = parentNode?.Id ?? string.Empty;

            BaseNode? node = nodeDto.Type switch
            {
                NodeTypeEnum.Start => new StartNode(),
                NodeTypeEnum.End => new EndNode(),
                NodeTypeEnum.Process => new StepNode(parentId, eventAggregator),
                NodeTypeEnum.Decision => CreateDecisionNode(nodeDto, eventAggregator, dialogService),
                NodeTypeEnum.Branch => new BranchNode(eventAggregator, parentNode),
                NodeTypeEnum.BranchStepList => new BranchStepListNodes(eventAggregator),
                _ => null
            };

            if (node != null)
            {
                RestoreBaseNodeProperties(nodeDto, node);
                RestoreSpecificNodeProperties(nodeDto, node, eventAggregator);
            }

            return node;
        }

        private DecisionNode CreateDecisionNode(NodeDto nodeDto, IEventAggregator eventAggregator, IDialogService? dialogService)
        {
            var decisionNode = new DecisionNode(eventAggregator, dialogService);

            // 清空默认创建的子分支
            decisionNode.Children.Clear();

            // 恢复子分支
            foreach (var childDto in nodeDto.Children)
            {
                if (childDto.Type == NodeTypeEnum.BranchStepList)
                {
                    var branchNode = new BranchStepListNodes(eventAggregator)
                    {
                        Parent = decisionNode
                    };

                    // 清空默认节点
                    branchNode.Nodes.Clear();
                    branchNode.Connections.Clear();

                    // 恢复分支的子节点
                    foreach (var branchChildDto in childDto.Children)
                    {
                        var childNode = CreateNodeFromDto(branchChildDto, branchNode, eventAggregator, dialogService);
                        if (childNode != null)
                        {
                            childNode.Parent = branchNode;
                            branchNode.Nodes.Add(childNode);

                            // 递归恢复
                            if (branchChildDto.Children.Count > 0)
                            {
                                RestoreNodeFromDto(branchChildDto, childNode, eventAggregator, dialogService);
                            }
                        }
                    }

                    // 添加AddOpNode
                    branchNode.Nodes.Add(new AddOpNode(branchNode));

                    // 恢复分支节点的基础属性
                    RestoreBaseNodeProperties(childDto, branchNode);

                    decisionNode.Children.Add(branchNode);
                }
            }

            // 恢复决策节点的特定属性
            if (nodeDto.Properties.TryGetValue("IsCollapse", out var isCollapse))
                decisionNode.IsCollapse = Convert.ToBoolean(isCollapse);

            if (nodeDto.Properties.TryGetValue("DecisionNum", out var decisionNum))
                decisionNode.DecisionNum = Convert.ToInt32(decisionNum);

            if (nodeDto.Properties.TryGetValue("ItemSpacing", out var itemSpacing))
                decisionNode.ItemSpacing = Convert.ToDouble(itemSpacing);

            return decisionNode;
        }

        private void RestoreBaseNodeProperties(NodeDto nodeDto, BaseNode node)
        {
            // 恢复基础属性（不恢复Id，保持新生成的Id或使用原Id）
            // node.Id = nodeDto.Id; // 可选：如果需要保持原Id
            node.Type = nodeDto.Type;
            node.Name = nodeDto.Name;
            node.Code = nodeDto.Code;
            node.Index = nodeDto.Index;
            node.Order = nodeDto.Order;
            node.Level = nodeDto.Level;
            node.Position = new Point(nodeDto.PositionX, nodeDto.PositionY);
            node.Width = nodeDto.Width;
            node.Height = nodeDto.Height;
        }

        private void RestoreSpecificNodeProperties(NodeDto nodeDto, BaseNode node, IEventAggregator eventAggregator)
        {
            switch (node)
            {
                case StepNode stepNode:
                    if (nodeDto.Properties.TryGetValue("Title", out var title))
                        stepNode.Title = title?.ToString() ?? string.Empty;

                    if (nodeDto.Properties.TryGetValue("TitleBackgroundColor", out var colorStr))
                    {
                        try
                        {
                            var color = (Color)ColorConverter.ConvertFromString(colorStr?.ToString() ?? "#FF9955");
                            stepNode.TitleBackground = new SolidColorBrush(color);
                        }
                        catch
                        {
                            stepNode.TitleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"));
                        }
                    }
                    break;

                case BranchNode branchNode:
                    if (nodeDto.Properties.TryGetValue("Title", out var branchTitle))
                        branchNode.Title = branchTitle?.ToString() ?? string.Empty;
                    break;
            }
        }

        #endregion
    }
}
