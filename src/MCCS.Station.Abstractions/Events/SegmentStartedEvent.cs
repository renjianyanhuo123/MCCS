using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 段开始事件
    /// </summary>
    public record SegmentStartedEvent : StationEvent
    {
        public Guid SegmentId { get; init; }
        public SegmentTypeEnum SegmentType { get; init; }
    }
}
