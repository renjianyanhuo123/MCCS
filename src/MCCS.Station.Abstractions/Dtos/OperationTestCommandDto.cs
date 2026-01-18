namespace MCCS.Station.Abstractions.Dtos;

/// <summary>
/// 操作试验命令 DTO
/// </summary>
/// <param name="RunningId"></param>
/// <param name="OperationStr"></param>
public record OperationTestCommandDto(string RunningId, string OperationStr);