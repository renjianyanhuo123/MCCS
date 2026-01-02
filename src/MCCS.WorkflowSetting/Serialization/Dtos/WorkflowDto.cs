namespace MCCS.WorkflowSetting.Serialization.Dtos;

/// <summary>
/// 工作流DTO - 顶层序列化对象
/// </summary>
public class WorkflowDto
{
    /// <summary>
    /// 工作流版本号（用于未来兼容性处理）
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 工作流唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 工作流名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工作流中的所有节点
    /// </summary>
    public List<BaseNodeDto> Nodes { get; set; } = [];
}