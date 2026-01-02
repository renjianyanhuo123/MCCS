using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization.Dtos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCCS.WorkflowSetting.Serialization.Converters
{
    /// <summary>
    /// BaseNodeDto的JSON转换器，用于处理多态类型的反序列化
    /// </summary>
    public class BaseNodeDtoConverter : JsonConverter<BaseNodeDto>
    {
        public override BaseNodeDto? ReadJson(JsonReader reader, Type objectType, BaseNodeDto? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null!;

            JObject jo = JObject.Load(reader);

            if (!jo.TryGetValue("Type", out var typeToken))
                throw new JsonSerializationException("Node missing Type field");

            var type = (NodeTypeEnum)typeToken.Value<int>();

            BaseNodeDto target = type switch
            {
                NodeTypeEnum.Start => new BaseNodeDto(),
                NodeTypeEnum.End => new BaseNodeDto(),

                NodeTypeEnum.Process => new StepNodeDto(),
                NodeTypeEnum.Branch => new BranchNodeDto(),
                NodeTypeEnum.BranchStepList => new BranchStepListDto(),
                NodeTypeEnum.Decision => new DecisionNodeDto(),

                _ => throw new NotSupportedException($"Unsupported node type: {type}")
            };

            serializer.Populate(jo.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, BaseNodeDto? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // 创建一个不包含此转换器的序列化器来避免递归
            var jObject = JObject.FromObject(value, new JsonSerializer
            {
                NullValueHandling = serializer.NullValueHandling,
                DefaultValueHandling = serializer.DefaultValueHandling,
                Formatting = serializer.Formatting
            });

            jObject.WriteTo(writer);
        }
    }
}
