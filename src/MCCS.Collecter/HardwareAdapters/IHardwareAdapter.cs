using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.DataAcquisition;

namespace MCCS.Collecter.HardwareAdapters;

/// <summary>
/// 硬件适配器接口
/// 抽象所有硬件操作，便于测试和扩展
/// </summary>
public interface IHardwareAdapter : IDisposable
{
    /// <summary>
    /// 设备 ID
    /// </summary>
    long DeviceId { get; }

    /// <summary>
    /// 设备名称
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// 连接状态
    /// </summary>
    HardwareConnectionStatus Status { get; }

    /// <summary>
    /// 连接状态流
    /// </summary>
    IObservable<HardwareConnectionStatus> StatusStream { get; }

    // ========== 连接管理 ==========

    /// <summary>
    /// 连接到硬件
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开硬件连接
    /// </summary>
    Task<bool> DisconnectAsync(CancellationToken cancellationToken = default);

    // ========== 数据读取 ==========

    /// <summary>
    /// 读取原始硬件数据 (同步，用于高速采集)
    /// </summary>
    RawHardwareData ReadData();

    /// <summary>
    /// 读取原始硬件数据 (异步)
    /// </summary>
    Task<RawHardwareData> ReadDataAsync(CancellationToken cancellationToken = default);

    // ========== 控制操作 ==========

    /// <summary>
    /// 设置控制模式
    /// </summary>
    Task<bool> SetControlModeAsync(SystemControlState controlMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置阀门状态
    /// </summary>
    Task<bool> SetValveStateAsync(bool isOpen, CancellationToken cancellationToken = default);

    /// <summary>
    /// 紧急停止
    /// </summary>
    Task<bool> EmergencyStopAsync(CancellationToken cancellationToken = default);
}
