using System.Buffers;
using System.IO.Pipes;
using MCCS.Infrastructure.Communication.NamedPipe.Handlers;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe;

/// <summary>
/// 命名管道服务端配置
/// </summary>
public sealed class NamedPipeServerOptions
{
    /// <summary>
    /// 管道名称
    /// </summary>
    public string PipeName { get; set; } = "MCCS_IPC_Pipe";

    /// <summary>
    /// 最大并发连接数
    /// </summary>
    public int MaxConcurrentConnections { get; set; } = 10;

    /// <summary>
    /// 接收缓冲区大小（字节）
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 65536;

    /// <summary>
    /// 发送缓冲区大小（字节）
    /// </summary>
    public int SendBufferSize { get; set; } = 65536;

    /// <summary>
    /// 请求处理超时时间（毫秒）
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 30000;
}

/// <summary>
/// 命名管道服务端 - 高性能请求响应服务
/// </summary>
public sealed class NamedPipeServer : IDisposable
{
    private readonly NamedPipeServerOptions _options;
    private readonly RequestRouter _router;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly List<Task> _connectionTasks = new();
    private readonly object _lock = new();

    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private bool _disposed;
    private int _activeConnections;

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections => _activeConnections;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _acceptTask != null && !_acceptTask.IsCompleted;

    /// <summary>
    /// 连接建立事件
    /// </summary>
    public event Action<string>? ClientConnected;

    /// <summary>
    /// 连接断开事件
    /// </summary>
    public event Action<string>? ClientDisconnected;

    /// <summary>
    /// 请求处理完成事件
    /// </summary>
    public event Action<string, PipeRequest, PipeResponse>? RequestProcessed;

    public NamedPipeServer(NamedPipeServerOptions? options = null, IMessageSerializer? serializer = null, ILogger? logger = null)
    {
        _options = options ?? new NamedPipeServerOptions();
        _serializer = serializer ?? new JsonMessageSerializer();
        _logger = logger;
        _router = new RequestRouter();
        _connectionSemaphore = new SemaphoreSlim(_options.MaxConcurrentConnections, _options.MaxConcurrentConnections);
    }

    /// <summary>
    /// 获取请求路由器（用于注册处理器）
    /// </summary>
    public RequestRouter Router => _router;

    /// <summary>
    /// 注册请求处理器
    /// </summary>
    public NamedPipeServer RegisterHandler(IRequestHandler handler)
    {
        _router.RegisterHandler(handler);
        return this;
    }

    /// <summary>
    /// 注册委托处理器
    /// </summary>
    public NamedPipeServer RegisterHandler(string route, Func<PipeRequest, CancellationToken, Task<PipeResponse>> handler)
    {
        _router.RegisterHandler(route, handler);
        return this;
    }

    /// <summary>
    /// 注册同步委托处理器
    /// </summary>
    public NamedPipeServer RegisterHandler(string route, Func<PipeRequest, PipeResponse> handler)
    {
        _router.RegisterHandler(route, handler);
        return this;
    }

    /// <summary>
    /// 启动服务端
    /// </summary>
    public async Task StartAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NamedPipeServer));
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        await AcceptConnectionsAsync(_cts.Token);
        _logger?.Information("Named pipe server started on pipe: {PipeName}", _options.PipeName);
    } 

    /// <summary>
    /// 停止服务端
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning) return;

        _logger?.Information("Stopping named pipe server...");
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

        _logger?.Information("Named pipe server stopped");
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
                Interlocked.Increment(ref _activeConnections);
                _logger?.Debug("Client connected: {ConnectionId}", connectionId);
                ClientConnected?.Invoke(connectionId);

                var connectionTask = HandleConnectionAsync(pipeServer, connectionId, cancellationToken);
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

    private async Task HandleConnectionAsync(NamedPipeServerStream pipeServer, string connectionId, CancellationToken cancellationToken)
    {
        try
        {
            using (pipeServer)
            {
                while (pipeServer.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var request = await ReadRequestAsync(pipeServer, cancellationToken);
                    if (request == null) break;

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(_options.RequestTimeoutMs);

                    var response = await _router.RouteAsync(request, timeoutCts.Token);
                    await WriteResponseAsync(pipeServer, response, cancellationToken);

                    RequestProcessed?.Invoke(connectionId, request, response);
                }
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
            _logger?.Error(ex, "Error handling connection {ConnectionId}", connectionId);
        }
        finally
        {
            Interlocked.Decrement(ref _activeConnections);
            _connectionSemaphore.Release();
            _logger?.Debug("Client disconnected: {ConnectionId}", connectionId);
            ClientDisconnected?.Invoke(connectionId);
        }
    }

    private async Task<PipeRequest?> ReadRequestAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
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

            return _serializer.DeserializeRequest(messageBuffer.AsSpan(0, messageLength).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(messageBuffer);
        }
    }

    private async Task WriteResponseAsync(NamedPipeServerStream pipe, PipeResponse response, CancellationToken cancellationToken)
    {
        var responseData = _serializer.SerializeResponse(response);
        var lengthBuffer = BitConverter.GetBytes(responseData.Length);

        await pipe.WriteAsync(lengthBuffer, cancellationToken);
        await pipe.WriteAsync(responseData, cancellationToken);
        await pipe.FlushAsync(cancellationToken);
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
    }
}
