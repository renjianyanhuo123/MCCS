using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Station.Host.Handlers;

[ApiNamedPipe("MCCS_Command_IPC")]
internal sealed class CommandHandler
{
    [Route("echo")]
    public PipeResponse Echo(PipeRequest request)
    {
        return PipeResponse.Success(request.RequestId, request.Payload);
    }

    [Route("calculate/add")]
    public Task<PipeResponse> Add(PipeRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(PipeResponse.Success(request.RequestId, ""));
    }
}
