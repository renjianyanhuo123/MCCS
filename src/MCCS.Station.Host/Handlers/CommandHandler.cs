using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Station.Host.Services;

namespace MCCS.Station.Host.Handlers;

/// <summary>
/// 命令处理器 - 作为薄层，将请求转发给 Services 处理
/// </summary>
[ApiNamedPipe("MCCS_Command_IPC")]
internal sealed class CommandHandler
{
    private readonly ICommandService _commandService;

    public CommandHandler(ICommandService commandService)
    {
        _commandService = commandService;
    }

    /// <summary>
    /// 启动测试指令
    /// </summary>
    [Route("startTestCommand")]
    public async Task<PipeResponse> StartCommandHandler(PipeRequest request, CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteTestCommandAsync(cancellationToken);
        return PipeResponse.Success(request.RequestId, result);
    }

    /// <summary>
    /// 阀门操作命令
    /// </summary>
    [Route("operationValveCommand")]
    public async Task<PipeResponse> OperationValveHandler(PipeRequest request, CancellationToken cancellationToken)
    {
        var result = await _commandService.ExecuteValveOperationAsync(request.RequestId, request.Payload, cancellationToken);
        return PipeResponse.Success(request.RequestId, result);
    }
}
