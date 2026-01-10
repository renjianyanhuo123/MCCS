using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 段完成事件
    /// </summary>
    public record SegmentCompletedEvent : StationEvent
    {
        public Guid SegmentId { get; init; }
        public StopReasonEnum StopReason { get; init; }
        public TimeSpan Duration { get; init; }
        public string Details { get; init; } = string.Empty;
    }
}
