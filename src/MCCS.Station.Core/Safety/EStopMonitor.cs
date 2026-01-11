using System.Reactive.Subjects;
using MCCS.Station.Abstractions.Events;
using MCCS.Station.Abstractions.Interfaces;

namespace MCCS.Station.Core.Safety;

/// <summary>
/// 急停监控器
/// 负责监控急停状态（硬件急停和软件急停）
/// 急停是最高优先级的安全措施，触发后必须人工干预
/// </summary>
public sealed class EStopMonitor : IEStopMonitor, IDisposable
{
    private readonly object _lock = new();
    private readonly Subject<EStopEvent> _estopTriggered = new(); 

    private volatile bool _isHardwareEStopActive;
    private volatile bool _isSoftwareEStopActive;
    private string? _lastHardwareSource;

    public IObservable<EStopEvent> EStopTriggered => _estopTriggered;

    public bool IsHardwareEStopActive
    {
        get
        {
            lock (_lock)
            {
                return _isHardwareEStopActive;
            }
        }
    }

    public bool IsSoftwareEStopActive
    {
        get
        {
            lock (_lock)
            {
                return _isSoftwareEStopActive;
            }
        }
    }

    public DateTime? LastEStopTime { get; private set; }

    public string? LastEStopReason { get; private set; } 

    public void UpdateHardwareEStopState(bool isActive, string source)
    { 
    }

    public async Task TriggerSoftwareEStopAsync(string reason, string triggeredBy)
    { 
        await Task.CompletedTask;
    }

    public async Task<(bool Success, string Message)> TryReleaseSoftwareEStopAsync(string operatorId, string reason)
    { 
        await Task.CompletedTask;
        return (true, "软件急停已成功释放");
    }

    public async Task<(bool Success, string Message)> TryResetAsync(string operatorId, string reason)
    {  
        await Task.CompletedTask;
        return (true, "急停系统已成功复位");
    } 

    public void Dispose()
    {
        _estopTriggered.OnCompleted();
        _estopTriggered.Dispose();
    }
}
