namespace MCCS.Core.Devices.Connections;

/// <summary>
/// 设备连接接口 - 负责底层通信
/// </summary>
public interface IDeviceConnection : IDisposable
{
    string ConnectionString { get; }
        
    /// <summary>
    /// 连接状态流
    /// </summary>
    IObservable<bool> ConnectionStateStream { get; }
        
    Task<bool> OpenAsync();
    Task<bool> CloseAsync();
    Task<byte[]> SendCommandAsync(byte[] command);
}