namespace MCCS.Core.Devices.Connections;

/// <summary>
/// 设备连接接口 - 负责底层通信
/// </summary>
public interface IDeviceConnection : IDisposable
{
    /// <summary>
    /// 连接ID - 唯一标识
    /// </summary>
    string ConnectionId { get; }
    /// <summary>
    /// 连接字符串 - 用于描述连接参数
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// 连接状态 - 是否已连接
    /// </summary>
    bool IsConnected { get; } 
    /// <summary>
    /// 打开连接 - 异步方法
    /// </summary>
    /// <returns></returns>
    Task<bool> OpenAsync();
    /// <summary>
    /// 关闭连接 - 异步方法
    /// </summary>
    /// <returns></returns>
    Task<bool> CloseAsync(); 
    /// <summary>
    /// 打开连接 - 同步方法
    /// </summary>
    /// <returns></returns>
    bool Open();
    /// <summary>
    /// 关闭连接 - 同步方法
    /// </summary>
    /// <returns></returns>
    bool Close();
}