namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 站点事件基类
    /// </summary>
    public abstract record StationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Source { get; init; } = string.Empty;
    }
}
