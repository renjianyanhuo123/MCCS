using System.Collections.Concurrent;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;
using Serilog;

namespace MCCS.Infrastructure.Communication.NamedPipe;

/// <summary>
/// 命名管道客户端连接池配置
/// </summary>
public sealed class NamedPipeClientPoolOptions
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
    /// 最小连接数
    /// </summary>
    public int MinConnections { get; set; } = 1;

    /// <summary>
    /// 最大连接数
    /// </summary>
    public int MaxConnections { get; set; } = 10;

    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 请求超时时间（毫秒）
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 获取连接等待超时（毫秒）
    /// </summary>
    public int AcquireTimeoutMs { get; set; } = 10000;

    /// <summary>
    /// 连接空闲超时（毫秒，超过此时间的空闲连接会被回收）
    /// </summary>
    public int IdleTimeoutMs { get; set; } = 60000;
}

/// <summary>
/// 命名管道客户端连接池 - 提供高并发支持
/// </summary>
public sealed class NamedPipeClientPool : IDisposable
{
    private readonly NamedPipeClientPoolOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger? _logger;
    private readonly ConcurrentBag<PooledClient> _availableClients = new();
    private readonly ConcurrentDictionary<string, PooledClient> _activeClients = new();
    private readonly SemaphoreSlim _poolSemaphore;
    private readonly Timer _cleanupTimer;

    private int _totalConnections;
    private bool _disposed;

    private class PooledClient
    {
        public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
        public NamedPipeClient Client { get; }
        public DateTime LastUsedTime { get; set; } = DateTime.UtcNow;

        public PooledClient(NamedPipeClient client)
        {
            Client = client;
        }
    }

    /// <summary>
    /// 当前总连接数
    /// </summary>
    public int TotalConnections => _totalConnections;

    /// <summary>
    /// 可用连接数
    /// </summary>
    public int AvailableConnections => _availableClients.Count;

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections => _activeClients.Count;

    public NamedPipeClientPool(NamedPipeClientPoolOptions? options = null, IMessageSerializer? serializer = null, ILogger? logger = null)
    {
        _options = options ?? new NamedPipeClientPoolOptions();
        _serializer = serializer ?? new JsonMessageSerializer();
        _logger = logger;
        _poolSemaphore = new SemaphoreSlim(_options.MaxConnections, _options.MaxConnections);
        _cleanupTimer = new Timer(CleanupIdleConnections, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// 预热连接池
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < _options.MinConnections; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var client = await CreateClientAsync(cancellationToken);
                if (client != null)
                {
                    _availableClients.Add(client);
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        _logger?.Information("Connection pool warmed up with {Count} connections", _availableClients.Count);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public async Task<PipeResponse> SendAsync(PipeRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NamedPipeClientPool));

        PooledClient? pooledClient = null;
        try
        {
            pooledClient = await AcquireClientAsync(cancellationToken);
            var response = await pooledClient.Client.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.Error(ex, "Error sending request through pool");
            if (pooledClient != null)
            {
                // 连接可能已损坏，移除并减少计数
                RemoveClient(pooledClient);
                pooledClient = null;
            }
            return PipeResponse.Failure(request.RequestId, PipeStatusCode.ConnectionError, ex.Message);
        }
        finally
        {
            if (pooledClient != null)
            {
                ReleaseClient(pooledClient);
            }
        }
    }

    /// <summary>
    /// 发送请求（简化版）
    /// </summary>
    public Task<PipeResponse> SendAsync(string route, string? payload = null, CancellationToken cancellationToken = default)
    {
        return SendAsync(PipeRequest.Create(route, payload), cancellationToken);
    }

    /// <summary>
    /// 发送强类型请求
    /// </summary>
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

    private async Task<PooledClient> AcquireClientAsync(CancellationToken cancellationToken)
    {
        // 先尝试从池中获取可用连接
        if (_availableClients.TryTake(out var client))
        {
            if (client.Client.IsConnected)
            {
                client.LastUsedTime = DateTime.UtcNow;
                _activeClients.TryAdd(client.Id, client);
                return client;
            }
            else
            {
                // 连接已断开，移除并创建新连接
                RemoveClient(client);
            }
        }

        // 等待可用连接槽位
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.AcquireTimeoutMs);

        if (!await _poolSemaphore.WaitAsync(_options.AcquireTimeoutMs, timeoutCts.Token))
        {
            throw new TimeoutException($"Failed to acquire connection from pool within {_options.AcquireTimeoutMs}ms");
        }

        try
        {
            // 再次尝试从池中获取
            if (_availableClients.TryTake(out client))
            {
                if (client.Client.IsConnected)
                {
                    client.LastUsedTime = DateTime.UtcNow;
                    _activeClients.TryAdd(client.Id, client);
                    _poolSemaphore.Release();
                    return client;
                }
                else
                {
                    RemoveClient(client);
                }
            }

            // 创建新连接
            var newClient = await CreateClientAsync(cancellationToken);
            if (newClient == null)
            {
                _poolSemaphore.Release();
                throw new InvalidOperationException("Failed to create new connection");
            }

            _activeClients.TryAdd(newClient.Id, newClient);
            _poolSemaphore.Release();
            return newClient;
        }
        catch
        {
            _poolSemaphore.Release();
            throw;
        }
    }

    private void ReleaseClient(PooledClient client)
    {
        _activeClients.TryRemove(client.Id, out _);
        client.LastUsedTime = DateTime.UtcNow;

        if (client.Client.IsConnected)
        {
            _availableClients.Add(client);
        }
        else
        {
            RemoveClient(client);
        }
    }

    private void RemoveClient(PooledClient client)
    {
        _activeClients.TryRemove(client.Id, out _);
        client.Client.Dispose();
        Interlocked.Decrement(ref _totalConnections);
    }

    private async Task<PooledClient?> CreateClientAsync(CancellationToken cancellationToken)
    {
        var clientOptions = new NamedPipeClientOptions
        {
            PipeName = _options.PipeName,
            ServerName = _options.ServerName,
            ConnectTimeoutMs = _options.ConnectTimeoutMs,
            RequestTimeoutMs = _options.RequestTimeoutMs,
            AutoReconnect = false // 池管理重连
        };

        var client = new NamedPipeClient(clientOptions, _serializer, _logger);

        try
        {
            await client.ConnectAsync(cancellationToken);
            Interlocked.Increment(ref _totalConnections);
            return new PooledClient(client);
        }
        catch (Exception ex)
        {
            _logger?.Warning(ex, "Failed to create pooled connection");
            client.Dispose();
            return null;
        }
    }

    private void CleanupIdleConnections(object? state)
    {
        if (_disposed) return;

        var now = DateTime.UtcNow;
        var idleThreshold = TimeSpan.FromMilliseconds(_options.IdleTimeoutMs);

        // 只清理超过最小连接数的空闲连接
        while (_availableClients.Count > _options.MinConnections && _availableClients.TryTake(out var client))
        {
            if (now - client.LastUsedTime > idleThreshold || !client.Client.IsConnected)
            {
                client.Client.Dispose();
                Interlocked.Decrement(ref _totalConnections);
                _logger?.Debug("Removed idle connection from pool: {Id}", client.Id);
            }
            else
            {
                // 还在有效期内，放回池中
                _availableClients.Add(client);
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cleanupTimer.Dispose();

        // 释放所有活跃连接
        foreach (var client in _activeClients.Values)
        {
            client.Client.Dispose();
        }
        _activeClients.Clear();

        // 释放所有可用连接
        while (_availableClients.TryTake(out var client))
        {
            client.Client.Dispose();
        }

        _poolSemaphore.Dispose();
    }
}
