using MCCS.Common.DataManagers.Devices;
using MCCS.Infrastructure.Models.StationSites;
using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.StationSites
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

        public BaseDevice? LinkedDevice { get; private set; }

        public void Link(BaseDevice device)
        {
            LinkedDevice = device;
        }
    }
}
