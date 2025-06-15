using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using MCCS.Core.Devices.Connections;

public abstract class BaseConnection: IDeviceConnection
{
    protected readonly BehaviorSubject<bool> _connectionStateSubject = new(false);
    public string ConnectionId { get; }

    public bool IsConnected { get; protected set; }

    public string ConnectionString { get; }

    public BaseConnection(string connectionString, string connectionId)
    {
        ConnectionId = connectionId;
        ConnectionString = connectionString;
    }

    public abstract Task<bool> OpenAsync();

    public abstract Task<bool> CloseAsync();  

    public abstract bool Open();

    public abstract bool Close();

    public void Dispose()
    {
        Close();
    }
}