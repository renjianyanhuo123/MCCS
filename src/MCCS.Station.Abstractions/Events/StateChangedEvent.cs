using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 状态变更事件
    /// </summary>
    public record StateChangedEvent : StationEvent
    {
        public StationStateEnum PreviousState { get; init; }
        public StationStateEnum CurrentState { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
