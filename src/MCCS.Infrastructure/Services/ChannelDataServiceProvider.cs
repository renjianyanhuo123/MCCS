namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据服务配置选项
/// </summary>
public class ChannelDataServiceOptions
{
    /// <summary>
    /// 共享内存通道名称
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// 最大缓冲项数
    /// </summary>
    public int MaxItems { get; set; } = 500;

    /// <summary>
    /// 轮询间隔（毫秒）
    /// </summary>
    public int PollIntervalMs { get; set; } = 10;

    /// <summary>
    /// 是否自动启动服务
    /// </summary>
    public bool AutoStart { get; set; } = true;
}

/// <summary>
/// 通道数据服务提供者
/// 提供跨项目的单例访问和生命周期管理
/// </summary>
public static class ChannelDataServiceProvider
{
    private static readonly object Lock = new();
    private static IChannelDataService? _instance;
    private static ChannelDataServiceOptions? _options;
    private static bool _isInitialized;

    /// <summary>
    /// 获取当前服务实例
    /// 如果服务未初始化，将使用默认配置创建实例
    /// </summary>
    public static IChannelDataService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    _instance ??= CreateService(_options ?? new ChannelDataServiceOptions());
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 服务是否已初始化
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// 使用指定配置初始化服务
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>服务实例</returns>
    public static IChannelDataService Initialize(ChannelDataServiceOptions? options = null)
    {
        lock (Lock)
        {
            if (_isInitialized && _instance != null)
            {
                return _instance;
            }

            _options = options ?? new ChannelDataServiceOptions();
            _instance = CreateService(_options);
            _isInitialized = true;

            return _instance;
        }
    }

    /// <summary>
    /// 使用配置委托初始化服务
    /// </summary>
    /// <param name="configure">配置委托</param>
    /// <returns>服务实例</returns>
    public static IChannelDataService Initialize(Action<ChannelDataServiceOptions> configure)
    {
        var options = new ChannelDataServiceOptions();
        configure(options);
        return Initialize(options);
    }

    /// <summary>
    /// 确保服务已启动
    /// </summary>
    public static async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
    {
        var service = Instance;
        if (!service.IsRunning)
        {
            await service.StartAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 停止并释放服务
    /// </summary>
    public static async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        lock (Lock)
        {
            if (_instance == null)
                return;
        }

        if (_instance.IsRunning)
        {
            await _instance.StopAsync(cancellationToken);
        }

        lock (Lock)
        {
            _instance.Dispose();
            _instance = null;
            _isInitialized = false;
        }
    }

    /// <summary>
    /// 重置服务（主要用于测试）
    /// </summary>
    public static void Reset()
    {
        lock (Lock)
        {
            _instance?.Dispose();
            _instance = null;
            _options = null;
            _isInitialized = false;
        }
    }

    private static IChannelDataService CreateService(ChannelDataServiceOptions options)
    {
        var service = new ChannelDataService(
            options.ChannelName,
            options.MaxItems,
            options.PollIntervalMs);

        if (options.AutoStart)
        {
            service.StartAsync().GetAwaiter().GetResult();
        }

        return service;
    }
}
