using MCCS.Station.Abstractions.Events;

namespace MCCS.Station.Abstractions.Interfaces;

/// <summary>
/// 急停监控器接口
/// 负责监控急停状态（硬件急停和软件急停）
/// 急停是最高优先级的安全措施，触发后必须人工干预
/// </summary>
public interface IEStopMonitor
{
    /// <summary>
    /// 急停事件流
    /// </summary>
    IObservable<EStopEvent> EStopTriggered { get; }

    /// <summary>
    /// 硬件急停是否激活
    /// </summary>
    bool IsHardwareEStopActive { get; }

    /// <summary>
    /// 软件急停是否激活
    /// </summary>
    bool IsSoftwareEStopActive { get; }

    /// <summary>
    /// 任何急停是否激活
    /// </summary>
    bool IsAnyEStopActive => IsHardwareEStopActive || IsSoftwareEStopActive; 

    /// <summary>
    /// 触发软件急停
    /// </summary>
    Task TriggerSoftwareEStopAsync(string reason, string triggeredBy);

    /// <summary>
    /// 尝试释放软件急停
    /// </summary>
    Task<(bool Success, string Message)> TryReleaseSoftwareEStopAsync(string operatorId, string reason);

    /// <summary>
    /// 尝试复位急停（包括确认硬件急停已释放）
    /// </summary>
    Task<(bool Success, string Message)> TryResetAsync(string operatorId, string reason);

    /// <summary>
    /// 更新硬件急停状态（由底层硬件监控调用）
    /// </summary>
    void UpdateHardwareEStopState(bool isActive, string source);

    /// <summary>
    /// 获取最后一次急停触发的时间
    /// </summary>
    DateTime? LastEStopTime { get; }

    /// <summary>
    /// 获取最后一次急停的原因
    /// </summary>
    string? LastEStopReason { get; }  
} 
