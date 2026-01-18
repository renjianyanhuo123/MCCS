using MCCS.Station.Abstractions.Dtos;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Services.IServices;

namespace MCCS.Station.Services.Services;

public class OperationTestService : IOperationTestService
{
    private readonly IControllerManager _controllerManager;

    public OperationTestService(IControllerManager controllerManager)
    {
        _controllerManager = controllerManager;
    }

    public bool Execute(OperationTestCommandDto commandDto)
    {
        if (string.Equals(commandDto.OperationStr, "start", StringComparison.OrdinalIgnoreCase))
        {
            return _controllerManager.OperationTest(true);
        }
        else if (string.Equals(commandDto.OperationStr, "stop", StringComparison.OrdinalIgnoreCase))
        {
            return _controllerManager.OperationTest(false);
        }
        else if (string.Equals(commandDto.OperationStr, "pause", StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Implement pause functionality
        }
        else if (string.Equals(commandDto.OperationStr, "continue", StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Implement continue functionality
        }
        else
        {
            throw new ArgumentException("Invalid operation string. Use 'start' or 'stop'.");
        } 
        return true;
    }
}