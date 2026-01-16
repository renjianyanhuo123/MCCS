using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using MCCS.Infrastructure.Communication;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

namespace MCCS.Station.Host.Communication;

/// <summary>
/// 共享内存数据发布服务
/// 订阅信号和虚拟通道数据流，并通过共享内存发布给客户端
/// </summary>
public sealed class SharedMemoryDataPublisher : IDataPublisher
{
    /// <summary>
    /// cpu亲和的事件循环调度器
    /// </summary>
    private readonly EventLoopScheduler _scheduler;

    private readonly ISignalManager _signalManager;
    private readonly IPseudoChannelManager _pseudoChannelManager;
    private readonly SharedMemoryChannelManager _channelManager;
    private readonly CompositeDisposable _subscriptions; 

    private SharedMemoryChannel<ChannelDataItem>? _dataChannel; 

    private CancellationTokenSource? _cts;  

    private volatile bool _isRunning;

    public bool IsRunning => _isRunning;

    /// <summary>
    /// 数据通道名称
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string DataChannelName = "MCCS_ChannelData"; 

    public SharedMemoryDataPublisher(
        ISignalManager signalManager,
        IPseudoChannelManager pseudoChannelManager)
    {
        _signalManager = signalManager;
        _pseudoChannelManager = pseudoChannelManager;
        _channelManager = new SharedMemoryChannelManager();
        _subscriptions = new CompositeDisposable();
        _scheduler = new EventLoopScheduler(ts => new Thread(ts)
        {
            Name = $"SharedMemory_PushData_1",
            IsBackground = true
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 初始化共享内存通道
        _dataChannel = _channelManager.GetOrCreateChannel<ChannelDataItem>(
            DataChannelName,
            SharedMemoryConstants.DefaultDataChannelMaxItems); 

        // 订阅所有虚拟通道数据
        SubscribeToPseudoChannels();
        _isRunning = true;

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SharedMemoryDataPublisher started");
#endif

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return; 
        _isRunning = false; 
        await _cts?.CancelAsync()!;  
        // 清理订阅
        _subscriptions.Clear();
#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] SharedMemoryDataPublisher stopped");
#endif
    }

    public void PublishChannelData(ref ChannelDataItem data)
    {
        if (_dataChannel == null || !_isRunning)
            return; 
        try
        { 
            _dataChannel.Write(ref data);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void PublishChannelDataBatch(IEnumerable<(long channelId, double value)> dataItems)
    {
        if (_dataChannel == null || !_isRunning)
            return;
        var packets = new List<ChannelDataItem>();
        foreach (var (channelId, value) in dataItems)
        {
            packets.Add(new ChannelDataItem
            {
                ChannelId = channelId,
                Value = value
            });
        }
        _dataChannel.WriteBatch(packets);
    }

    /*
     *var merged =
           Observable.Merge(...)
               .Publish()
               .RefCount();
       
       var shared =
           merged
               .Publish()
               .RefCount();
       
       var gauge$ =
           shared
               .Buffer(TimeSpan.FromMilliseconds(200))
               .Select(buf => buf[^1]);
       
       var chart$ =
           shared
               .Buffer(TimeSpan.FromMilliseconds(200));
       
       gauge$.Subscribe();
       chart$.Subscribe(); 
     */
    private void SubscribeToPseudoChannels()
    {
        var pseudoChannels = _pseudoChannelManager.GetPseudoChannels();
        var merged =
            _pseudoChannelManager.GetPseudoChannels()
                .Select(channel =>
                    channel
                        .GetPseudoChannelStream()
                        .Select(data => new ChannelDataItem
                        {
                            ChannelId = channel.ChannelId,
                            SequenceIndex = data.SequenceIndex,
                            Value = data.Value
                        })
                        .ObserveOn(_scheduler)
                )
                .Merge()
                .Publish()
                .RefCount();
        // 创建共享的热Observable(这里相当于共享上面的合并的流数据)
        //var shared =
        //    merged
        //        .Publish()
        //        .RefCount();
        var subscription = merged.Subscribe(dataPoint => PublishChannelData(ref dataPoint),
            HandleError,
            () => { /* completed */ }); // 启动合并流 
        _subscriptions.Add(subscription);

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Subscribed to {pseudoChannels.Count()} pseudo channels");
#endif
    } 

    private void HandleError(Exception ex)
    {
#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error in channel: {ex.Message}");
#endif
    }
      

    public void Dispose()
    {
        if (_isRunning)
        {
            StopAsync().GetAwaiter().GetResult();
        }

        _subscriptions.Dispose();
        _channelManager.Dispose();
        _cts?.Dispose();
    }
}
