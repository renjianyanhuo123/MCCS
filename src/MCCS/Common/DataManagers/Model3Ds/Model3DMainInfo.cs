using MCCS.Station.Abstractions.Models;

namespace MCCS.Common.DataManagers.Model3Ds
{
    public class Model3DMainInfo(long id, string modelFileKey, string name)
    {
        public long Id { get; private set; } = id;
        /// <summary>
        /// 单个模型的Key
        /// </summary>
        public string ModelFileKey { get; private set; } = modelFileKey;

        public string Name { get; private set; } = name;
        /// <summary>
        /// 某个模型绑定的所有控制通道
        /// </summary>
        public List<StationSiteControlChannelInfo> ControlChannelInfos { get; set; } = [];
        /// <summary>
        /// 映射匹配的设备
        /// </summary>
        public long? MappingDeviceId { get; set; }
    }
}
