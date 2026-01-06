using MCCS.Infrastructure.Models.StationSites;

using Newtonsoft.Json;

namespace MCCS.Station.Abstractions.Models
{
    [method: JsonConstructor]
    public sealed class StationSiteControllerSignalInfo(
        long id,
        long belongControllerId,
        string name)
    {
        public long Id { get; } = id;
        public string Name { get; } = name; 
        public long BelongControllerId { get; } = belongControllerId;
        /// <summary>
        /// 控制通道信号类型
        /// </summary>
        public SignalTypeEnum ControlChannelSignalType { get; set; }

        public BaseDevice? LinkedDevice { get; set; } 
    }
}
