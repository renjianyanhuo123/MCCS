using MCCS.Station.Abstractions.Dtos;

namespace MCCS.Station.Services.IServices;

public interface IOperationTestService
{
    /// <summary>
    /// 操作试验命令 DTO
    /// </summary>
    /// <param name="commandDto"></param>
    /// <returns></returns>
    bool Execute(OperationTestCommandDto commandDto);
}