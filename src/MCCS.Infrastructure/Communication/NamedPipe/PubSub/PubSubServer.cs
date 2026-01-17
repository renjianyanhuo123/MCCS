using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipes;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe.PubSub;

/// <summary>
/// 发布-订阅服务端配置
/// </summary>
public sealed class PubSubServerOptions
{
    /// <summary>
    /// 管道名称
    /// </summary>
    public string PipeName { get; set; } = "MCCS_PubSub_Pipe";

    /// <summary>
    /// 最大并发连接数
    /// </summary>
    public int MaxConcurrentConnections { get; set; } = 50;

    /// <summary>
    /// 接收缓冲区大小（字节）
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 65536;

    /// <summary>
    /// 发送缓冲区大小（字节）
    /// </summary>
    public int SendBufferSize { get; set; } = 65536;

    /// <summary>
    /// 发布消息超时时间（毫秒）
    /// </summary>
    public int PublishTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 是否启用消息持久化（暂不实现）
    /// </summary>
    public bool EnablePersistence { get; set; } = false;
}

/// <summary>
/// 发布-订阅服务端 - 基于命名管道的发布订阅服务
/// </summary>
public sealed class PubSubServer : IDisposable
{
    private readonly PubSubServerOptions _options;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IPubSubMessageSerializer _serializer;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly ConcurrentDictionary<string, ClientConnection> _connections = new();
    private readonly List<Task> _connectionTasks = new();
    private readonly object _lock = new();

    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private bool _disposed;
    private int _activeConnections;

    /// <summary>
    /// 客户端连接信息
    /// </summary>
    private sealed class ClientConnection : IDisposable
    {
        public string ConnectionId { get; }
        public NamedPipeServerStream PipeStream { get; }
        public string? SubscriberId { get; set; }
        public SemaphoreSlim WriteLock { get; } = new(1, 1);

        public ClientConnection(string connectionId, NamedPipeServerStream pipeStream)
        {
            ConnectionId = connectionId;
            PipeStream = pipeStream;
        }

        public void Dispose()
        {
            WriteLock.Dispose();
            PipeStream.Dispose();
        }
    }

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections => _activeConnections;

    /// <summary>
    /// 管道名称
    /// </summary>
    public string PipeName => _options.PipeName;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _acceptTask != null && !_acceptTask.IsCompleted;

    /// <summary>
    /// 订阅管理器
    /// </summary>
    public ISubscriptionManager SubscriptionManager => _subscriptionManager;

    /// <summary>
    /// 客户端连接事件
    /// </summary>
    public event Action<string>? ClientConnected;

    /// <summary>
    /// 客户端断开事件
    /// </summary>
    public event Action<string>? ClientDisconnected;

    /// <summary>
    /// 订阅事件
    /// </summary>
    public event Action<string, string>? Subscribed;

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public event Action<string, string>? Unsubscribed;

    /// <summary>
    /// 消息发布事件
    /// </summary>
    public event Action<string, int>? MessagePublished;

    public PubSubServer(
        PubSubServerOptions? options = null,
        IPubSubMessageSerializer? serializer = null,
        ISubscriptionManager? subscriptionManager = null,
        ILogger? logger = null)
    {
        _options = options ?? new PubSubServerOptions();
        _serializer = serializer ?? new JsonPubSubMessageSerializer();
        _subscriptionManager = subscriptionManager ?? new SubscriptionManager();
        _logger = logger;
        _connectionSemaphore = new SemaphoreSlim(_options.MaxConcurrentConnections, _options.MaxConcurrentConnections);
    }

    /// <summary>
    /// 启动服务端
    /// </summary>
    public Task StartAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PubSubServer));
        if (IsRunning) return Task.CompletedTask;

        _cts = new CancellationTokenSource();
        _acceptTask = AcceptConnectionsAsync(_cts.Token);
        _logger?.Information("PubSub server started on pipe: {PipeName}", _options.PipeName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 停止服务端
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning) return;

        _logger?.Information("Stopping PubSub server...");
        await _cts?.CancelAsync()!;

        // 等待接受任务完成
        if (_acceptTask != null)
        {
            try
            {
                await _acceptTask;
            }
            catch (OperationCanceledException)
            {
                // 预期的取消
            }
        }

        // 等待所有连接处理完成
        Task[] tasks;
        lock (_lock)
        {
            tasks = _connectionTasks.ToArray();
            _connectionTasks.Clear();
        }

        if (tasks.Length > 0)
        {
            await Task.WhenAll(tasks);
        }

        // 清理连接
        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }
        _connections.Clear();

        _subscriptionManager.Clear();

        _logger?.Information("PubSub server stopped");
    }

    /// <summary>
    /// 发布消息到指定主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">消息负载</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送成功的订阅者数量</returns>
    public async Task<int> PublishAsync(string topic, string? payload, CancellationToken cancellationToken = default)
    {
        var message = PublishMessage.Create(topic, payload);
        return await PublishAsync(message, cancellationToken);
    }

    /// <summary>
    /// 发布消息到指定主题（泛型版本）
    /// </summary>
    public async Task<int> PublishAsync<T>(string topic, T data, CancellationToken cancellationToken = default)
    {
        var message = PublishMessage.Create(topic, _serializer.Serialize(data));
        return await PublishAsync(message, cancellationToken);
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    public async Task<int> PublishAsync(PublishMessage message, CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PubSubServer));

        var subscribers = _subscriptionManager.GetSubscribers(message.Topic);
        if (subscribers.Count == 0)
        {
            _logger?.Debug("No subscribers for topic: {Topic}", message.Topic);
            return 0;
        }

        var successCount = 0;
        var failedSubscribers = new List<string>();

        // 并行发送给所有订阅者
        var tasks = subscribers.Select(async sub =>
        {
            try
            {
                if (sub.PipeStream?.IsConnected == true)
                {
                    await SendMessageAsync(sub, message, cancellationToken);
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    lock (failedSubscribers)
                    {
                        failedSubscribers.Add(sub.SubscriberId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning(ex, "Failed to send message to subscriber: {SubscriberId}", sub.SubscriberId);
                lock (failedSubscribers)
                {
                    failedSubscribers.Add(sub.SubscriberId);
                }
            }
        });

        await Task.WhenAll(tasks);

        // 清理失败的订阅者
        foreach (var subscriberId in failedSubscribers)
        {
            _subscriptionManager.RemoveSubscriber(subscriberId);
        }

        _logger?.Debug("Published message to {Count} subscribers for topic: {Topic}", successCount, message.Topic);
        MessagePublished?.Invoke(message.Topic, successCount);

        return successCount;
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _connectionSemaphore.WaitAsync(cancellationToken);

            try
            {
                var pipeServer = new NamedPipeServerStream(
                    _options.PipeName,
                    PipeDirection.InOut,
                    _options.MaxConcurrentConnections,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    _options.ReceiveBufferSize,
                    _options.SendBufferSize);

                await pipeServer.WaitForConnectionAsync(cancellationToken);

                var connectionId = Guid.NewGuid().ToString("N")[..8];
                var connection = new ClientConnection(connectionId, pipeServer);
                _connections.TryAdd(connectionId, connection);

                Interlocked.Increment(ref _activeConnections);
                _logger?.Debug("Client connected: {ConnectionId}", connectionId);
                ClientConnected?.Invoke(connectionId);

                var connectionTask = HandleConnectionAsync(connection, cancellationToken);
                lock (_lock)
                {
                    _connectionTasks.Add(connectionTask);
                }

                // 清理已完成的连接任务
                _ = CleanupCompletedTasksAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _connectionSemaphore.Release();
                break;
            }
            catch (Exception ex)
            {
                _connectionSemaphore.Release();
                _logger?.Error(ex, "Error accepting connection");
            }
        }
    }

    private async Task HandleConnectionAsync(ClientConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            while (connection.PipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var envelope = await ReadMessageAsync(connection.PipeStream, cancellationToken);
                if (envelope == null) break;

                await ProcessMessageAsync(connection, envelope, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 预期的取消
        }
        catch (IOException)
        {
            // 客户端断开连接
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error handling connection {ConnectionId}", connection.ConnectionId);
        }
        finally
        {
            // 清理订阅
            if (!string.IsNullOrEmpty(connection.SubscriberId))
            {
                _subscriptionManager.RemoveSubscriber(connection.SubscriberId);
            }

            _connections.TryRemove(connection.ConnectionId, out _);
            Interlocked.Decrement(ref _activeConnections);
            _connectionSemaphore.Release();

            _logger?.Debug("Client disconnected: {ConnectionId}", connection.ConnectionId);
            ClientDisconnected?.Invoke(connection.ConnectionId);

            connection.Dispose();
        }
    }

    private async Task ProcessMessageAsync(ClientConnection connection, PipeMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        switch (envelope.MessageType)
        {
            case PipeMessageType.Subscribe:
                await HandleSubscribeAsync(connection, envelope.Payload, cancellationToken);
                break;

            case PipeMessageType.Unsubscribe:
                await HandleUnsubscribeAsync(connection, envelope.Payload, cancellationToken);
                break;

            default:
                _logger?.Warning("Unexpected message type received: {MessageType}", envelope.MessageType);
                break;
        }
    }

    private async Task HandleSubscribeAsync(ClientConnection connection, string payload, CancellationToken cancellationToken)
    {
        var message = _serializer.Deserialize<SubscribeMessage>(payload);
        if (message == null)
        {
            _logger?.Warning("Invalid subscribe message");
            return;
        }

        // 使用连接ID作为订阅者ID，或使用客户端指定的ID
        var subscriberId = message.SubscriberId ?? connection.ConnectionId;
        connection.SubscriberId = subscriberId;

        _subscriptionManager.Subscribe(subscriberId, message.Topic, connection.PipeStream);

        _logger?.Information("Subscriber {SubscriberId} subscribed to topic: {Topic}", subscriberId, message.Topic);
        Subscribed?.Invoke(subscriberId, message.Topic);

        // 发送订阅确认
        var ack = SubscribeAckMessage.Succeeded(message.Topic, subscriberId, message.MessageId);
        await SendAckAsync(connection, ack, cancellationToken);
    }

    private async Task HandleUnsubscribeAsync(ClientConnection connection, string payload, CancellationToken cancellationToken)
    {
        var message = _serializer.Deserialize<UnsubscribeMessage>(payload);
        if (message == null)
        {
            _logger?.Warning("Invalid unsubscribe message");
            return;
        }

        var subscriberId = message.SubscriberId ?? connection.SubscriberId ?? connection.ConnectionId;
        _subscriptionManager.Unsubscribe(subscriberId, message.Topic);

        _logger?.Information("Subscriber {SubscriberId} unsubscribed from topic: {Topic}", subscriberId, message.Topic ?? "all");
        Unsubscribed?.Invoke(subscriberId, message.Topic ?? "all");

        // 发送取消订阅确认
        var ack = UnsubscribeAckMessage.Succeeded(message.Topic, message.MessageId);
        await SendAckAsync(connection, ack, cancellationToken);
    }

    private async Task<PipeMessageEnvelope?> ReadMessageAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        // 读取消息长度（4字节）
        var lengthBuffer = new byte[4];
        var bytesRead = await pipe.ReadAsync(lengthBuffer, cancellationToken);
        if (bytesRead < 4) return null;

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
        if (messageLength <= 0 || messageLength > _options.ReceiveBufferSize)
        {
            _logger?.Warning("Invalid message length: {Length}", messageLength);
            return null;
        }

        // 读取消息体
        var messageBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
        try
        {
            var totalRead = 0;
            while (totalRead < messageLength)
            {
                bytesRead = await pipe.ReadAsync(messageBuffer.AsMemory(totalRead, messageLength - totalRead), cancellationToken);
                if (bytesRead == 0) return null;
                totalRead += bytesRead;
            }

            return _serializer.DeserializeEnvelope(messageBuffer.AsSpan(0, messageLength).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(messageBuffer);
        }
    }

    private async Task SendMessageAsync(SubscriptionInfo subscription, PublishMessage message, CancellationToken cancellationToken)
    {
        if (subscription.PipeStream?.IsConnected != true) return;

        var envelope = PipeMessageEnvelope.Create(PipeMessageType.Publish, _serializer.Serialize(message));
        var data = _serializer.SerializeEnvelope(envelope);
        var lengthBuffer = BitConverter.GetBytes(data.Length);

        await subscription.WriteLock.WaitAsync(cancellationToken);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.PublishTimeoutMs);

            await subscription.PipeStream.WriteAsync(lengthBuffer, timeoutCts.Token);
            await subscription.PipeStream.WriteAsync(data, timeoutCts.Token);
            await subscription.PipeStream.FlushAsync(timeoutCts.Token);
        }
        finally
        {
            subscription.WriteLock.Release();
        }
    }

    private async Task SendAckAsync(ClientConnection connection, PubSubMessageBase ack, CancellationToken cancellationToken)
    {
        var envelope = PipeMessageEnvelope.Create(ack.MessageType, _serializer.SerializeObject(ack));
        var data = _serializer.SerializeEnvelope(envelope);
        var lengthBuffer = BitConverter.GetBytes(data.Length);

        await connection.WriteLock.WaitAsync(cancellationToken);
        try
        {
            await connection.PipeStream.WriteAsync(lengthBuffer, cancellationToken);
            await connection.PipeStream.WriteAsync(data, cancellationToken);
            await connection.PipeStream.FlushAsync(cancellationToken);
        }
        finally
        {
            connection.WriteLock.Release();
        }
    }

    private async Task CleanupCompletedTasksAsync()
    {
        await Task.Delay(100); // 延迟清理，避免频繁锁操作

        lock (_lock)
        {
            _connectionTasks.RemoveAll(t => t.IsCompleted);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();
        _connectionSemaphore.Dispose();

        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }
        _connections.Clear();
    }
}
