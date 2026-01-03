using System.Text.Json;
using System.Text.Json.Serialization;
using MCCS.Workflow.StepComponents.Core;
using MCCS.Workflow.StepComponents.Registry;

namespace MCCS.Workflow.StepComponents.Serialization
{
    /// <summary>
    /// 组件序列化器实现
    /// </summary>
    public class ComponentSerializer : IComponentSerializer
    {
        private readonly IComponentRegistry _registry;
        private readonly JsonSerializerOptions _jsonOptions;

        public ComponentSerializer(IComponentRegistry registry)
        {
            _registry = registry;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public ComponentInstanceDto Serialize(IStepComponent component, string? instanceId = null)
        {
            return new ComponentInstanceDto
            {
                InstanceId = instanceId ?? Guid.NewGuid().ToString("N"),
                ComponentId = component.Id,
                DisplayName = component.Name,
                ParameterValues = ConvertToSerializableValues(component.GetParameterValues()),
                ModifiedAt = DateTime.Now
            };
        }

        public string SerializeToJson(IStepComponent component, string? instanceId = null)
        {
            var dto = Serialize(component, instanceId);
            return JsonSerializer.Serialize(dto, _jsonOptions);
        }

        public IStepComponent? Deserialize(ComponentInstanceDto dto)
        {
            var component = _registry.CreateComponent(dto.ComponentId);
            if (component == null)
            {
                return null;
            }

            var convertedValues = ConvertFromSerializableValues(dto.ParameterValues);
            component.SetParameterValues(convertedValues);

            return component;
        }

        public IStepComponent? DeserializeFromJson(string json)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ComponentInstanceDto>(json, _jsonOptions);
                return dto != null ? Deserialize(dto) : null;
            }
            catch
            {
                return null;
            }
        }

        public List<ComponentInstanceDto> SerializeMany(IEnumerable<IStepComponent> components)
        {
            return components.Select(c => Serialize(c)).ToList();
        }

        public List<IStepComponent> DeserializeMany(IEnumerable<ComponentInstanceDto> dtos)
        {
            return dtos
                .Select(dto => Deserialize(dto))
                .Where(c => c != null)
                .Cast<IStepComponent>()
                .ToList();
        }

        /// <summary>
        /// 转换为可序列化的值
        /// </summary>
        private static Dictionary<string, object?> ConvertToSerializableValues(IDictionary<string, object?> values)
        {
            var result = new Dictionary<string, object?>();

            foreach (var kvp in values)
            {
                result[kvp.Key] = ConvertToSerializable(kvp.Value);
            }

            return result;
        }

        private static object? ConvertToSerializable(object? value)
        {
            if (value == null) return null;

            var type = value.GetType();

            // 基本类型直接返回
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                return value;
            }

            // DateTime转为ISO字符串
            if (type == typeof(DateTime))
            {
                return ((DateTime)value).ToString("O");
            }

            // 枚举转为字符串
            if (type.IsEnum)
            {
                return value.ToString();
            }

            // 集合类型序列化为JSON
            if (value is System.Collections.IEnumerable enumerable && type != typeof(string))
            {
                return JsonSerializer.Serialize(value);
            }

            // 其他复杂类型序列化为JSON
            return JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// 从序列化值转换回来
        /// </summary>
        private static Dictionary<string, object?> ConvertFromSerializableValues(Dictionary<string, object?> values)
        {
            var result = new Dictionary<string, object?>();

            foreach (var kvp in values)
            {
                result[kvp.Key] = ConvertFromSerializable(kvp.Value);
            }

            return result;
        }

        private static object? ConvertFromSerializable(object? value)
        {
            if (value == null) return null;

            // JsonElement需要特殊处理
            if (value is JsonElement jsonElement)
            {
                return ConvertJsonElement(jsonElement);
            }

            return value;
        }

        private static object? ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
                _ => element.GetRawText()
            };
        }
    }
}
