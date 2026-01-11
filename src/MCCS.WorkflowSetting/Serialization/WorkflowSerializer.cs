using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Converters;
using MCCS.WorkflowSetting.Serialization.Dtos;

using Newtonsoft.Json;

namespace MCCS.WorkflowSetting.Serialization
{
    /// <summary>
    /// 工作流序列化服务实现
    /// </summary>
    public class WorkflowSerializer : IWorkflowSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            Converters = { new BaseNodeDtoConverter() }
        };

        /// <summary>
        /// 将StepListNodes转换为可序列化的DTO对象，并序列化为JSON字符串
        /// </summary>
        public string Serialize(StepListNodes stepListNodes)
        {
            var dto = ToDto(stepListNodes);
            return JsonConvert.SerializeObject(dto, _serializerSettings);
        }

        /// <summary>
        /// 将StepListNodes转换为DTO对象
        /// </summary>
        public WorkflowDto ToDto(StepListNodes stepListNodes)
        {
            var dto = new WorkflowDto
            {
                Id = stepListNodes.Id,
                Name = stepListNodes.Name,
                Version = "1.0",
                Nodes = []
            };

            // 转换所有节点
            foreach (var node in stepListNodes.Nodes)
            {
                var nodeDto = NodeToDtoConverter.Convert(node);
                if (nodeDto != null)
                {
                    dto.Nodes.Add(nodeDto);
                }
            }

            return dto;
        }

        /// <summary>
        /// 将JSON字符串反序列化为StepListNodes
        /// </summary>
        public StepListNodes Deserialize(string json, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            var dto = JsonConvert.DeserializeObject<WorkflowDto>(json, _serializerSettings);
            if (dto == null) throw new InvalidOperationException("Failed to deserialize workflow JSON.");
            return FromDto(dto, eventAggregator, dialogService);
        }

        /// <summary>
        /// 将DTO对象转换为StepListNodes
        /// </summary>
        public StepListNodes FromDto(WorkflowDto workflowDto, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            // 创建转换器
            var converter = new DtoToNodeConverter(eventAggregator, dialogService);
            var children = workflowDto.Nodes
                .Select(nodeDto => converter.Convert(nodeDto, null))
                .OfType<BaseNode>().ToList();
            // 转换所有节点
            var stepListNodes = new StepListNodes(eventAggregator, dialogService, children)
            {
                Name = workflowDto.Name
            };
            // 在业务节点后插入AddOpNode
            InsertAddOpNodes(stepListNodes);
            return stepListNodes;
        }

        /// <summary>
        /// 在业务节点之后插入AddOpNode
        /// </summary>
        private static void InsertAddOpNodes(StepListNodes listNodes)
        {
            var nodesToInsert = new List<(int index, AddOpNode node)>();
            for (var i = 0; i < listNodes.Nodes.Count; i++)
            {
                var currentNode = listNodes.Nodes[i];
                switch (currentNode)
                {
                    // 在StepNode、DecisionNode后添加AddOpNode（StartNode和EndNode不需要）
                    case StepNode or DecisionNode:
                    // StartNode后面也需要添加AddOpNode
                    case StartNode:
                        nodesToInsert.Add((i + 1, new AddOpNode(listNodes)));
                        break;
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
    }
}
