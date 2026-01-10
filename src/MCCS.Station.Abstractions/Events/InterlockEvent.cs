using MCCS.Station.Abstractions.Enums;

namespace MCCS.Station.Abstractions.Events
{
    /// <summary>
    /// 联锁事件
    /// </summary>
    public record InterlockEvent : StationEvent
    {
        public InterlockTypeEnum InterlockType { get; init; }
        public bool IsTripped { get; init; }
        public string Description { get; init; } = string.Empty;
    }
}
