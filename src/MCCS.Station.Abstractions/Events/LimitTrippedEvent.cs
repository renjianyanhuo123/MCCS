namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 限位触发事件
    /// </summary>
    public record LimitTrippedEvent : StationEvent
    {
        public string ChannelId { get; init; } = string.Empty;
        public string SignalName { get; init; } = string.Empty;
        public double Threshold { get; init; }
        public double ActualValue { get; init; }
        public bool IsUpperLimit { get; init; }
        public Guid? SegmentId { get; init; }
    }
}
