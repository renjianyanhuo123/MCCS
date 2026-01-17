using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Station.Host.Handlers;

[ApiNamedPipe("MCCS_Command_IPC")]
internal sealed class CommandHandler
{ 
    /// <summary>
    /// 开始试验指令
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Route("startTestCommand")]
    public async Task<PipeResponse> StartCommandHandler(PipeRequest request, CancellationToken cancellationToken)
    {
        // 模拟一个异步操作
        await Task.Delay(1000, cancellationToken);
        // 返回成功响应
        return PipeResponse.Success(request.RequestId, "Test command started successfully.");
    }

    [Route("operationValveCommand")]
    public async Task<PipeResponse> OperationValveHandler(PipeRequest request, CancellationToken cancellationToken) 
    {
        // 模拟一个异步操作
        await Task.Delay(200, cancellationToken);
        // Console.WriteLine($"{request.RequestId}{request.}");
        // 返回成功响应
        return PipeResponse.Success(request.RequestId, "Test command started successfully.");
    }
}
