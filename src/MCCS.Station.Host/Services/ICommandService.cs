namespace MCCS.Station.Host.Services;

/// <summary>
/// 命令服务接口 - 处理所有命令相关的业务逻辑
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// 执行测试命令
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果消息</returns>
    Task<string> ExecuteTestCommandAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行阀门操作命令
    /// </summary>
    /// <param name="requestId">请求ID</param>
    /// <param name="payload">命令负载</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果消息</returns>
    Task<string> ExecuteValveOperationAsync(string requestId, string? payload, CancellationToken cancellationToken = default);
}
