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
    public override async Task<bool> OpenAsync()
    {
        try
        {
            await Task.Delay(100); // 模拟连接延迟 
            IsConnected = true;
            return true;
        }
        catch (Exception)
        { 
            throw;
        }
    }

    public override async Task<bool> CloseAsync()
    {
        await Task.Delay(100);
        IsConnected = false;
        return true;
    }

    public override bool Open()
    {
        IsConnected = true;
        return true;
    }

    public override bool Close()
    {
        IsConnected = false;
        return true;
    }
}
