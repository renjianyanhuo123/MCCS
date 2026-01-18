using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Station.Abstractions.Dtos;
using MCCS.Station.Services.IServices;

using Newtonsoft.Json;

namespace MCCS.Station.Host.Handlers;

/// <summary>
/// 命令处理器 - 作为薄层，将请求转发给 Services 处理
/// </summary>
[ApiNamedPipe("MCCS_Command_IPC")]
internal sealed class CommandHandler
{
    private readonly IOperationValveService _operationValveService;
    private readonly IOperationTestService _operationTestService;

    public CommandHandler(
        IOperationValveService operationValveService, 
        IOperationTestService operationTestService)
    {
        _operationValveService = operationValveService;
        _operationTestService = operationTestService;
    }

    /// <summary>
    /// 操作试验指令
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("operationTestCommand")]
    public PipeResponse OperationTestCommandHandler(PipeRequest request, CancellationToken cancellationToken)
    {
        var dto = JsonConvert.DeserializeObject<OperationTestCommandDto>(request.Payload ?? "");
        if (dto == null) return PipeResponse.Failure(request.RequestId, PipeStatusCode.InvalidRequest);
        var result = _operationTestService.Execute(dto);
        return result ? PipeResponse.Success(request.RequestId, null) :
            PipeResponse.Failure(request.RequestId, PipeStatusCode.UnknownError, "Failed to execute test operation command.");
    }

    /// <summary>
    /// 阀门操作命令
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("operationValveCommand")]
    public PipeResponse OperationValveHandler(PipeRequest request, CancellationToken cancellationToken)
    {
        var dto = JsonConvert.DeserializeObject<OperationValveCommandDto>(request.Payload ?? "");
        if (dto == null) return PipeResponse.Failure(request.RequestId, PipeStatusCode.InvalidRequest);
        var result = _operationValveService.Execute(dto);
        return result ? PipeResponse.Success(request.RequestId, null) : 
            PipeResponse.Failure(request.RequestId, PipeStatusCode.UnknownError, "Failed to execute valve operation command.");
    }
}
