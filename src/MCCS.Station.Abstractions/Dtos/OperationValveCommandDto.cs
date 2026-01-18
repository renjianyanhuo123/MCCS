namespace MCCS.Station.Abstractions.Dtos
{
    /// <summary>
    /// 操作阀门命令DTO
    /// </summary>
    /// <param name="ControlChannelId">控制通道ID</param>
    /// <param name="OperationValve">操作阀门 true: 打开; false: 关闭</param>
    public record OperationValveCommandDto(long ControlChannelId, bool OperationValve);
}
