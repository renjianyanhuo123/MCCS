namespace MCCS.Core.Devices.Connections;

public class SerialPortConnection : BaseConnection
{
    public SerialPortConnection(string connectionString) : base(connectionString)
    {
    }

    public override Task<bool> OpenAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CloseAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<byte[]> SendCommandAsync(byte[] command)
    {
        throw new NotImplementedException();
    }
}