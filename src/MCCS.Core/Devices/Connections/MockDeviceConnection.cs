namespace MCCS.Core.Devices.Connections;

/// <summary>
/// 模拟演示的链接
/// </summary>
public class MockDeviceConnection : BaseConnection
{
    public MockDeviceConnection(string connectionString) : base(connectionString)
    {
    }

    public override async Task<bool> OpenAsync()
    {
        await Task.Delay(100); // 模拟连接延迟
        _connectionStateSubject.OnNext(true);
        return true;
    }

    public override async Task<bool> CloseAsync()
    {
        await Task.Delay(100);
        _connectionStateSubject.OnNext(false);
        return true;
    }

    public override async Task<byte[]> SendCommandAsync(byte[] command)
    {
        await Task.Delay(10); // 模拟通信延迟
        return [0x01, 0x02, 0x03, 0x04]; // 模拟响应
    }
}
