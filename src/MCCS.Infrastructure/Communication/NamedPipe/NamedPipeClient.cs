using System.Buffers;
using System.IO.Pipes;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe;

/// <summary>
/// 命名管道客户端配置
/// </summary>
public sealed class NamedPipeClientOptions
{
    /// <summary>
    /// 管道名称
    /// </summary>
    public string PipeName { get; set; } = "MCCS_IPC_Pipe";

    /// <summary>
    /// 服务器名称（本机为"."）
    /// </summary>
    public string ServerName { get; set; } = ".";

    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 请求超时时间（毫秒）
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 30000;

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
    public int MaxReconnectAttempts { get; set; } = 3;
}

/// <summary>
/// 命名管道客户端 - 高性能请求响应客户端
/// </summary>
public sealed class NamedPipeClient(
    NamedPipeClientOptions? options = null,
    IMessageSerializer? serializer = null,
    ILogger? logger = null)
    : IDisposable
{
    private readonly NamedPipeClientOptions _options = options ?? new NamedPipeClientOptions();
    private readonly IMessageSerializer _serializer = serializer ?? new JsonMessageSerializer();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private NamedPipeClientStream? _pipeClient;
    private bool _disposed;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _pipeClient?.IsConnected ?? false;

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    public event Action<bool>? ConnectionStateChanged;

    /// <summary>
    /// 连接到服务端
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NamedPipeClient));
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
            logger?.Debug("Connected to named pipe server: {PipeName}", _options.PipeName);
            ConnectionStateChanged?.Invoke(true);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Connection timeout after {_options.ConnectTimeoutMs}ms");
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        if (_pipeClient != null)
        {
            _pipeClient.Dispose();
            _pipeClient = null;
            logger?.Debug("Disconnected from named pipe server");
            ConnectionStateChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// 发送请求并等待响应
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public async Task<PipeResponse> SendAsync(PipeRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NamedPipeClient));

        var attempts = 0;
        while (true)
        {
            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync(cancellationToken);
                }

                return await SendInternalAsync(request, cancellationToken);
            }
            catch (IOException ex) when (_options.AutoReconnect && attempts < _options.MaxReconnectAttempts)
            {
                attempts++;
                logger?.Warning(ex, "Connection lost, attempting reconnect ({Attempt}/{Max})", attempts, _options.MaxReconnectAttempts);

                Disconnect();
                await Task.Delay(_options.ReconnectIntervalMs, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger?.Error(ex, "Failed to send request: {Route}", request.Route);
                return PipeResponse.Failure(request.RequestId, PipeStatusCode.ConnectionError, ex.Message);
            }
        }
    }

    /// <summary>
    /// 发送请求（简化版）
    /// </summary>
    /// <param name="route">路由</param>
    /// <param name="payload">负载数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public Task<PipeResponse> SendAsync(string route, string? payload = null, CancellationToken cancellationToken = default) => SendAsync(PipeRequest.Create(route, payload), cancellationToken);

    /// <summary>
    /// 发送强类型请求
    /// </summary>
    /// <typeparam name="TRequest">请求类型</typeparam>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="route">路由</param>
    /// <param name="request">请求数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应数据</returns>
    public async Task<TResponse?> SendAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken cancellationToken = default)
    {
        var pipeRequest = PipeRequest.Create(route, _serializer.Serialize(request));
        var response = await SendAsync(pipeRequest, cancellationToken);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Payload))
        {
            return default;
        }

        return _serializer.Deserialize<TResponse>(response.Payload);
    }

    private async Task<PipeResponse> SendInternalAsync(PipeRequest request, CancellationToken cancellationToken)
    {
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            if (_pipeClient is not { IsConnected: true })
            {
                return PipeResponse.Failure(request.RequestId, PipeStatusCode.ConnectionError, "Not connected to server");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.RequestTimeoutMs);

            // 发送请求
            await WriteRequestAsync(_pipeClient, request, timeoutCts.Token);

            // 读取响应
            return await ReadResponseAsync(_pipeClient, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.Timeout, $"Request timeout after {_options.RequestTimeoutMs}ms");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task WriteRequestAsync(NamedPipeClientStream pipe, PipeRequest request, CancellationToken cancellationToken)
    {
        var requestData = _serializer.SerializeRequest(request);
        var lengthBuffer = BitConverter.GetBytes(requestData.Length);

        await pipe.WriteAsync(lengthBuffer, cancellationToken);
        await pipe.WriteAsync(requestData, cancellationToken);
        await pipe.FlushAsync(cancellationToken);
    }

    private async Task<PipeResponse> ReadResponseAsync(NamedPipeClientStream pipe, CancellationToken cancellationToken)
    {
        // 读取消息长度（4字节）
        var lengthBuffer = new byte[4];
        var bytesRead = await pipe.ReadAsync(lengthBuffer, cancellationToken);
        if (bytesRead < 4)
        {
            throw new IOException("Connection closed while reading response length");
        }

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
        if (messageLength <= 0 || messageLength > _options.ReceiveBufferSize)
        {
            throw new InvalidOperationException($"Invalid response length: {messageLength}");
        }

        // 读取消息体
        var messageBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
        try
        {
            var totalRead = 0;
            while (totalRead < messageLength)
            {
                bytesRead = await pipe.ReadAsync(messageBuffer.AsMemory(totalRead, messageLength - totalRead), cancellationToken);
                if (bytesRead == 0)
                {
                    throw new IOException("Connection closed while reading response body");
                }
                totalRead += bytesRead;
            }

            return _serializer.DeserializeResponse(messageBuffer.AsSpan(0, messageLength).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(messageBuffer);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Disconnect();
        _sendLock.Dispose();
    }
}
