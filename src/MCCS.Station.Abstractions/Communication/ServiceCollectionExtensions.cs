using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Station.Abstractions.Communication;

/// <summary>
/// 通信服务DI扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加共享内存数据接收服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="pollIntervalMs">数据轮询间隔（毫秒）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddStationDataReceiver(
        this IServiceCollection services,
        int pollIntervalMs = 10)
    {
        services.AddSingleton(sp => new StationDataReceiver(pollIntervalMs));
        services.AddSingleton<IDataReceiver>(sp => sp.GetRequiredService<StationDataReceiver>());
        return services;
    }

    /// <summary>
    /// 添加共享内存数据接收服务（配置选项）
    /// </summary>
    public static IServiceCollection AddStationDataReceiver(
        this IServiceCollection services,
        Action<StationDataReceiverOptions> configureOptions)
    {
        var options = new StationDataReceiverOptions();
        configureOptions(options);

        services.AddSingleton(sp => new StationDataReceiver(options.PollIntervalMs));
        services.AddSingleton<IDataReceiver>(sp => sp.GetRequiredService<StationDataReceiver>());
        return services;
    }
}

/// <summary>
/// 数据接收器配置选项
/// </summary>
public class StationDataReceiverOptions
{
    /// <summary>
    /// 数据轮询间隔（毫秒）
    /// </summary>
    public int PollIntervalMs { get; set; } = 10;

    /// <summary>
    /// 心跳超时时间（毫秒）
    /// </summary>
    public int HeartbeatTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 是否自动重连
    /// </summary>
    public bool AutoReconnect { get; set; } = true;
}
