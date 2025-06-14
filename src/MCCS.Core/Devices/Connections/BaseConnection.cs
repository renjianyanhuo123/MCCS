using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using MCCS.Core.Devices.Connections;

public abstract class BaseConnection: IDeviceConnection
{
    private readonly Channel<bool> _connectionStatusChannel;
    private readonly Channel<byte[]> _dataReceivedChannel;

    protected readonly BehaviorSubject<bool> _connectionStateSubject = new(false);
    public string ConnectionId { get; }

    public bool IsConnected { get; protected set; }

    public string ConnectionString { get; }
    public IObservable<bool> ConnectionStateStream { get; } 
    public IObservable<byte[]> DataReceived { get; }

    public BaseConnection(string connectionString, string connectionId)
    {
        ConnectionId = connectionId;
        ConnectionString = connectionString;

        _connectionStatusChannel = Channel.CreateUnbounded<bool>();
        _dataReceivedChannel = Channel.CreateUnbounded<byte[]>();

        ConnectionStateStream = Observable.Create<bool>(async (observer, ct) =>
        {
            await foreach (var status in _connectionStatusChannel.Reader.ReadAllAsync(ct))
                observer.OnNext(status);
        });

        DataReceived = Observable.Create<byte[]>(async (observer, ct) =>
        {
            await foreach (var data in _dataReceivedChannel.Reader.ReadAllAsync(ct))
                observer.OnNext(data);
        });
    }

    public abstract Task<bool> OpenAsync();

    public abstract Task<bool> CloseAsync();

    public abstract Task<byte[]> SendCommandAsync(byte[] command);

    protected void OnConnectionStatusChanged(bool isConnected)
    {
        IsConnected = isConnected;
        _connectionStatusChannel.Writer.TryWrite(isConnected);
    }

    protected void OnDataReceived(byte[] data)
    {
        _dataReceivedChannel.Writer.TryWrite(data);
    }

    public void Dispose()
    {
        Close();
        _connectionStatusChannel.Writer.TryComplete();
        _dataReceivedChannel.Writer.TryComplete();
    }

    public virtual bool Open()
    {
        return true;
    }

    public virtual bool Close()
    {
        return true;
    }

    public virtual byte[] SendCommand(byte[] command)
    {
        return [];
    }
}