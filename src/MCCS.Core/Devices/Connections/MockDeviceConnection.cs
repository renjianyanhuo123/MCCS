using MCCS.Core.Devices.Details;
using System;
using System.Text;
using System.Text.Json;

namespace MCCS.Core.Devices.Connections;



/// <summary>
/// 模拟演示的链接
/// </summary>
public class MockDeviceConnection(string connectionString, string connectionId) : BaseConnection(connectionString, connectionId)
{
    private static readonly Random _rand = new();

    // 生成符合 N(0, 1) 的随机数（标准正态分布）
    private static double NextStandardNormal()
    {
        var u1 = 1.0 - _rand.NextDouble(); // 避免 log(0)
        var u2 = 1.0 - _rand.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    // 生成符合 N(mean, stdDev^2) 的随机数
    private static double NextNormal(double mean, double stdDev)
    {
        return mean + stdDev * NextStandardNormal();
    }

    public override async Task<bool> OpenAsync()
    {
        try
        {
            await Task.Delay(100); // 模拟连接延迟
            OnConnectionStatusChanged(true);
            return true;
        }
        catch (Exception)
        {
            OnConnectionStatusChanged(false);
            throw;
        }
    }

    public override async Task<bool> CloseAsync()
    {
        await Task.Delay(100);
        OnConnectionStatusChanged(false);
        return true;
    }

    public override async Task<byte[]> SendCommandAsync(byte[] command)
    {
        await Task.Delay(10); // 模拟通信延迟
        var mockData = new MockActuatorCollection
        {
            Force = NextNormal(10, 8),
            Displacement = NextNormal(9, 8)
        };
        var json = JsonSerializer.Serialize(mockData); // 模拟数据处理
        var byteStr = Encoding.UTF8.GetBytes(json);
        OnDataReceived(byteStr);
        return byteStr; // 模拟响应
    }
}
