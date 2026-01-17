using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipes;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe.PubSub;

/// <summary>
/// 发布-订阅客户端配置
/// </summary>
public sealed class PubSubClientOptions
{
    /// <summary>
    /// 管道名称
    /// </summary>
    public string PipeName { get; set; } = "MCCS_PubSub_Pipe";

    /// <summary>
    /// 服务器名称（本机为"."）
    /// </summary>
    public string ServerName { get; set; } = ".";

    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 接收缓冲区大小（字节）
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 65536;

    /// <summary>
    /// 自动重连
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// 重连间隔（毫秒）
    /// </summary>
    public int ReconnectIntervalMs { get; set; } = 1000;

    /// <summary>
    /// 最大重连次数（0表示无限）
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 5;

    /// <summary>
    /// 订阅者ID（可选，由服务端分配）
    /// </summary>
    public string? SubscriberId { get; set; }
}

/// <summary>
/// 发布-订阅客户端 - 订阅者
/// </summary>
public sealed class PubSubClient : IDisposable
{
    private readonly PubSubClientOptions _options;
    private readonly IPubSubMessageSerializer _serializer;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly ConcurrentDictionary<string, List<Action<PublishMessage>>> _topicHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<PubSubMessageBase>> _pendingAcks = new();

    private NamedPipeClientStream? _pipeClient;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveTask;
    private bool _disposed;
    private string? _subscriberId;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _pipeClient?.IsConnected ?? false;

    /// <summary>
    /// 订阅者ID
    /// </summary>
    public string? SubscriberId => _subscriberId;

    /// <summary>
    /// 已订阅的主题
    /// </summary>
    public IReadOnlyCollection<string> SubscribedTopics => _topicHandlers.Keys.ToList();

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    public event Action<bool>? ConnectionStateChanged;

    /// <summary>
    /// 消息接收事件（所有主题）
    /// </summary>
    public event Action<PublishMessage>? MessageReceived;

    /// <summary>
    /// 错误事件
    /// </summary>
    public event Action<Exception>? Error;

    public PubSubClient(
        PubSubClientOptions? options = null,
        IPubSubMessageSerializer? serializer = null,
        ILogger? logger = null)
    {
        _options = options ?? new PubSubClientOptions();
        _serializer = serializer ?? new JsonPubSubMessageSerializer();
        _logger = logger;
        _subscriberId = _options.SubscriberId;
    }

    /// <summary>
    /// 连接到服务端
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PubSubClient));
        if (IsConnected) return;

        _pipeClient = new NamedPipeClientStream(
            _options.ServerName,
            _options.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.ConnectTimeoutMs);

        try
        {
            await _pipeClient.ConnectAsync(timeoutCts.Token);
            _logger?.Debug("Connected to PubSub server: {PipeName}", _options.PipeName);
            ConnectionStateChanged?.Invoke(true);

            // 启动接收循环
            _receiveCts = new CancellationTokenSource();
            _receiveTask = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Connection timeout after {_options.ConnectTimeoutMs}ms");
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_receiveCts != null)
        {
            await _receiveCts.CancelAsync();
            _receiveCts.Dispose();
            _receiveCts = null;
        }

        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask;
            }
            catch (OperationCanceledException)
            {
                // 预期的取消
            }
            _receiveTask = null;
        }

        if (_pipeClient != null)
        {
            _pipeClient.Dispose();
            _pipeClient = null;
            _logger?.Debug("Disconnected from PubSub server");
            ConnectionStateChanged?.Invoke(false);
        }

        // 清理待处理的确认
        foreach (var tcs in _pendingAcks.Values)
        {
            tcs.TrySetCanceled();
        }
        _pendingAcks.Clear();
    }

    /// <summary>
    /// 订阅主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="handler">消息处理器</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task SubscribeAsync(string topic, Action<PublishMessage> handler, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(handler);

        if (_disposed) throw new ObjectDisposedException(nameof(PubSubClient));
        if (!IsConnected) throw new InvalidOperationException("Not connected to server");

        // 添加处理器
        var handlers = _topicHandlers.GetOrAdd(topic, _ => new List<Action<PublishMessage>>());
        lock (handlers)
        {
            handlers.Add(handler);
        }

        // 发送订阅请求
        var message = SubscribeMessage.Create(topic, _subscriberId);
        var ack = await SendAndWaitAckAsync<SubscribeAckMessage>(message, cancellationToken);

        if (!ack.Success)
        {
            // 移除处理器
            lock (handlers)
            {
                handlers.Remove(handler);
            }
            throw new InvalidOperationException($"Subscribe failed: {ack.ErrorMessage}");
        }

        // 更新订阅者ID
        _subscriberId = ack.SubscriberId;
        _logger?.Information("Subscribed to topic: {Topic}", topic);
    }

    /// <summary>
    /// 订阅主题（泛型版本）
    /// </summary>
    public async Task SubscribeAsync<T>(string topic, Action<T?> handler, CancellationToken cancellationToken = default)
    {
        await SubscribeAsync(topic, msg =>
        {
            if (string.IsNullOrEmpty(msg.Payload))
            {
                handler(default);
            }
            else
            {
                var data = _serializer.Deserialize<T>(msg.Payload);
                handler(data);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// 取消订阅主题
    /// </summary>
    /// <param name="topic">主题（为空则取消所有订阅）</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task UnsubscribeAsync(string? topic = null, CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PubSubClient));
        if (!IsConnected) throw new InvalidOperationException("Not connected to server");

        // 发送取消订阅请求
        var message = UnsubscribeMessage.Create(topic, _subscriberId);
        var ack = await SendAndWaitAckAsync<UnsubscribeAckMessage>(message, cancellationToken);

        if (!ack.Success)
        {
            throw new InvalidOperationException($"Unsubscribe failed: {ack.ErrorMessage}");
        }

        // 移除处理器
        if (string.IsNullOrEmpty(topic))
        {
            _topicHandlers.Clear();
        }
        else
        {
            _topicHandlers.TryRemove(topic, out _);
        }

        _logger?.Information("Unsubscribed from topic: {Topic}", topic ?? "all");
    }

    private async Task<TAck> SendAndWaitAckAsync<TAck>(PubSubMessageBase message, CancellationToken cancellationToken)
        where TAck : PubSubMessageBase
    {
        var tcs = new TaskCompletionSource<PubSubMessageBase>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingAcks.TryAdd(message.MessageId, tcs);

        try
        {
            await SendMessageAsync(message, cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(5000); // 5秒超时

            var registration = timeoutCts.Token.Register(() => tcs.TrySetCanceled());
            try
            {
                var ack = await tcs.Task;
                return (TAck)ack;
            }
            finally
            {
                await registration.DisposeAsync();
            }
        }
        finally
        {
            _pendingAcks.TryRemove(message.MessageId, out _);
        }
    }

    private async Task SendMessageAsync(PubSubMessageBase message, CancellationToken cancellationToken)
    {
        if (_pipeClient is not { IsConnected: true })
        {
            throw new InvalidOperationException("Not connected to server");
        }

        var envelope = PipeMessageEnvelope.Create(message.MessageType, _serializer.SerializeObject(message));
        var data = _serializer.SerializeEnvelope(envelope);
        var lengthBuffer = BitConverter.GetBytes(data.Length);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await _pipeClient.WriteAsync(lengthBuffer, cancellationToken);
            await _pipeClient.WriteAsync(data, cancellationToken);
            await _pipeClient.FlushAsync(cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var reconnectAttempts = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!IsConnected)
                {
                    if (!_options.AutoReconnect || (_options.MaxReconnectAttempts > 0 && reconnectAttempts >= _options.MaxReconnectAttempts))
                    {
                        break;
                    }

                    reconnectAttempts++;
                    _logger?.Warning("Connection lost, attempting reconnect ({Attempt}/{Max})",
                        reconnectAttempts, _options.MaxReconnectAttempts == 0 ? "∞" : _options.MaxReconnectAttempts.ToString());

                    await Task.Delay(_options.ReconnectIntervalMs, cancellationToken);
                    await ConnectAsync(cancellationToken);

                    // 重新订阅
                    await ResubscribeAllAsync(cancellationToken);
                    reconnectAttempts = 0;
                }

                var envelope = await ReadMessageAsync(cancellationToken);
                if (envelope == null)
                {
                    // 连接断开
                    _pipeClient?.Dispose();
                    _pipeClient = null;
                    ConnectionStateChanged?.Invoke(false);
                    continue;
                }

                ProcessMessage(envelope);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (IOException)
            {
                // 连接断开
                _pipeClient?.Dispose();
                _pipeClient = null;
                ConnectionStateChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in receive loop");
                Error?.Invoke(ex);
            }
        }
    }

    private async Task ResubscribeAllAsync(CancellationToken cancellationToken)
    {
        var topics = _topicHandlers.Keys.ToList();
        foreach (var topic in topics)
        {
            try
            {
                var message = SubscribeMessage.Create(topic, _subscriberId);
                await SendMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.Warning(ex, "Failed to resubscribe to topic: {Topic}", topic);
            }
        }
    }

    private async Task<PipeMessageEnvelope?> ReadMessageAsync(CancellationToken cancellationToken)
    {
        if (_pipeClient is not { IsConnected: true }) return null;

        // 读取消息长度（4字节）
        var lengthBuffer = new byte[4];
        var bytesRead = await _pipeClient.ReadAsync(lengthBuffer, cancellationToken);
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
                bytesRead = await _pipeClient.ReadAsync(messageBuffer.AsMemory(totalRead, messageLength - totalRead), cancellationToken);
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

    private void ProcessMessage(PipeMessageEnvelope envelope)
    {
        switch (envelope.MessageType)
        {
            case PipeMessageType.Publish:
                HandlePublishMessage(envelope.Payload);
                break;

            case PipeMessageType.SubscribeAck:
                HandleSubscribeAck(envelope.Payload);
                break;

            case PipeMessageType.UnsubscribeAck:
                HandleUnsubscribeAck(envelope.Payload);
                break;

            default:
                _logger?.Warning("Unexpected message type: {MessageType}", envelope.MessageType);
                break;
        }
    }

    private void HandlePublishMessage(string payload)
    {
        var message = _serializer.Deserialize<PublishMessage>(payload);
        if (message == null)
        {
            _logger?.Warning("Invalid publish message");
            return;
        }

        // 触发全局事件
        MessageReceived?.Invoke(message);

        // 触发主题处理器
        if (_topicHandlers.TryGetValue(message.Topic, out var handlers))
        {
            List<Action<PublishMessage>> handlersCopy;
            lock (handlers)
            {
                handlersCopy = handlers.ToList();
            }

            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler(message);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Error in message handler for topic: {Topic}", message.Topic);
                }
            }
        }
    }

    private void HandleSubscribeAck(string payload)
    {
        var ack = _serializer.Deserialize<SubscribeAckMessage>(payload);
        if (ack == null) return;

        if (_pendingAcks.TryGetValue(ack.MessageId, out var tcs))
        {
            tcs.TrySetResult(ack);
        }
    }

    private void HandleUnsubscribeAck(string payload)
    {
        var ack = _serializer.Deserialize<UnsubscribeAckMessage>(payload);
        if (ack == null) return;

        if (_pendingAcks.TryGetValue(ack.MessageId, out var tcs))
        {
            tcs.TrySetResult(ack);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _pipeClient?.Dispose();
        _writeLock.Dispose();

        foreach (var tcs in _pendingAcks.Values)
        {
            tcs.TrySetCanceled();
        }
        _pendingAcks.Clear();
    }
}
