using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Disposables;

using MCCS.Infrastructure.Communication;
using MCCS.Station.Abstractions.Communication;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MCCS.Station.Host.Communication;

/// <summary>
/// 共享内存数据发布服务
/// 订阅信号和虚拟通道数据流，并通过共享内存发布给客户端
/// </summary>
public sealed class SharedMemoryDataPublisher : IDataPublisher
{
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

    public void PublishChannelData(long channelId, double value)
    {
        if (_dataChannel == null || !_isRunning)
            return;

        try
        {
            var packet = new ChannelDataItem()
            {
                ChannelId = channelId,
                Value = value
            };
            _dataChannel.Write(packet);
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

    private void SubscribeToPseudoChannels()
    {
        var pseudoChannels = _pseudoChannelManager.GetPseudoChannels(); 
        foreach (var channel in pseudoChannels)
        {
            var channelId = channel.ChannelId;

            // 订阅虚拟通道数据流
            var subscription = channel.GetPseudoChannelStream()
                .Subscribe(
                    dataPoint => PublishChannelData(channelId, dataPoint.Value),
                    ex => HandleError(channelId, ex),
                    () => { /* completed */ });

            _subscriptions.Add(subscription);
        }

#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Subscribed to {pseudoChannels.Count()} pseudo channels");
#endif
    } 

    private void HandleError(long channelId, Exception ex)
    {
#if DEBUG
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error in channel {channelId}: {ex.Message}");
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
