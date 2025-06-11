using System.Reactive.Subjects;
using MCCS.Core.Devices.Connections;

public abstract class BaseConnection: IDeviceConnection
{
    protected readonly BehaviorSubject<bool> _connectionStateSubject = new(false);
    
    public string ConnectionString { get; }
    public IObservable<bool> ConnectionStateStream => _connectionStateSubject;

    public BaseConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public abstract Task<bool> OpenAsync();

    public abstract Task<bool> CloseAsync();

    public abstract Task<byte[]> SendCommandAsync(byte[] command);
    
    public void Dispose()
    {
        _connectionStateSubject.OnNext(false);
        _connectionStateSubject.Dispose();
    }
}