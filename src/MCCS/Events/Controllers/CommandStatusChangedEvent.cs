using MCCS.Infrastructure.TestModels.CommandTracking;
using Prism.Events;

namespace MCCS.Events.Controllers;

/// <summary>
/// 命令状态变更事件
/// </summary>
public class CommandStatusChangedEvent : PubSubEvent<CommandStatusChangedEventParam>
{
}

/// <summary>
/// 命令状态变更事件参数
/// </summary>
public class CommandStatusChangedEventParam
{
    /// <summary>
    /// 命令记录
    /// </summary>
    public required CommandRecord CommandRecord { get; init; }
}
