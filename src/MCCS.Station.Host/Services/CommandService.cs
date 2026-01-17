namespace MCCS.Station.Host.Services;

/// <summary>
/// 命令服务实现 - 处理所有命令相关的业务逻辑
/// </summary>
internal sealed class CommandService : ICommandService
{
    /// <inheritdoc/>
    public async Task<string> ExecuteTestCommandAsync(CancellationToken cancellationToken = default)
    {
        // 模拟一个异步操作
        await Task.Delay(1000, cancellationToken);
        return "Test command started successfully.";
    }

    /// <inheritdoc/>
    public async Task<string> ExecuteValveOperationAsync(string requestId, string? payload, CancellationToken cancellationToken = default)
    {
        // 模拟一个异步操作
        await Task.Delay(200, cancellationToken);
        Console.WriteLine($"{requestId}:{payload}");
        return "Valve operation completed successfully.";
    }
}
