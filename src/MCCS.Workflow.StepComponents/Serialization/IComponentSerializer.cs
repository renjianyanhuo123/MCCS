using MCCS.Workflow.StepComponents.Core;

namespace MCCS.Workflow.StepComponents.Serialization
{
    /// <summary>
    /// 组件序列化器接口
    /// </summary>
    public interface IComponentSerializer
    {
        /// <summary>
        /// 将组件实例序列化为DTO
        /// </summary>
        ComponentInstanceDto Serialize(IStepComponent component, string? instanceId = null);

        /// <summary>
        /// 将组件实例序列化为JSON字符串
        /// </summary>
        string SerializeToJson(IStepComponent component, string? instanceId = null);

        /// <summary>
        /// 从DTO反序列化为组件实例
        /// </summary>
        IStepComponent? Deserialize(ComponentInstanceDto dto);

        /// <summary>
        /// 从JSON字符串反序列化为组件实例
        /// </summary>
        IStepComponent? DeserializeFromJson(string json);

        /// <summary>
        /// 批量序列化组件列表
        /// </summary>
        List<ComponentInstanceDto> SerializeMany(IEnumerable<IStepComponent> components);

        /// <summary>
        /// 批量反序列化组件列表
        /// </summary>
        List<IStepComponent> DeserializeMany(IEnumerable<ComponentInstanceDto> dtos);
    }
}
