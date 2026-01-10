using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 告警事件
    /// </summary>
    public record AlarmEvent : StationEvent
    {
        public AlarmLevelEnum Level { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}
