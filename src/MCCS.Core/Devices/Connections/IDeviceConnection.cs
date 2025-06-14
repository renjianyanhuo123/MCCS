namespace MCCS.Core.Devices.Connections;

/// <summary>
/// 设备连接接口 - 负责底层通信
/// </summary>
public interface IDeviceConnection : IDisposable
{
    /// <summary>
    /// 连接ID - 唯一标识
    /// </summary>
    public string ConnectionId { get; }

    public string ConnectionString { get; }

    /// <summary>
    /// 连接状态 - 是否已连接
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// 连接状态流
    /// </summary>
    public IObservable<bool> ConnectionStateStream { get; }
    /// <summary>
    /// 数据接收流 - 接收到的数据
    /// </summary>
    public IObservable<byte[]> DataReceived { get; }
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
    /// 发送指令到设备 - 异步方法
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    Task<byte[]> SendCommandAsync(byte[] command);
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
    /// <summary>
    /// 发送指令到设备 - 同步方法
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    byte[] SendCommand(byte[] command);
}