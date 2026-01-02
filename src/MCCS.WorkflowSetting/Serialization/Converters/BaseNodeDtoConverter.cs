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
                return null;

            var jObject = JObject.Load(reader);

            // 根据Type字段确定具体类型
            var typeValue = jObject["Type"]?.Value<int>() ?? 0;
            var nodeType = (NodeTypeEnum)typeValue;

            BaseNodeDto? dto = nodeType switch
            {
                NodeTypeEnum.Start => new BaseNodeDto(),
                NodeTypeEnum.End => new BaseNodeDto(),
                NodeTypeEnum.Process => new StepNodeDto(),
                NodeTypeEnum.Branch => new BranchNodeDto(),
                NodeTypeEnum.Decision => new DecisionNodeDto(),
                NodeTypeEnum.BranchStepList => new BranchStepListDto(),
                _ => new BaseNodeDto()
            };

            if (dto != null)
            {
                // 使用新的序列化器避免递归调用
                var newSerializer = new JsonSerializer();
                foreach (var converter in serializer.Converters)
                {
                    if (converter != this)
                    {
                        newSerializer.Converters.Add(converter);
                    }
                }

                using var subReader = jObject.CreateReader();
                newSerializer.Populate(subReader, dto);
            }

            return dto;
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
