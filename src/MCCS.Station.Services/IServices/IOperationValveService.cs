using MCCS.Station.Abstractions.Dtos;

namespace MCCS.Station.Services.IServices
{
    public interface IOperationValveService
    {
        bool Execute(OperationValveCommandDto commandDto);
    }
}
