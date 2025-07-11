using MCCS.Core.Models.SystemManager;

namespace MCCS.Core.Domain
{
    public class ChannelAggregate( 
        ChannelInfo channelInfo,
        List<VariableInfo> variableInfos,
        List<HardwareInfo> hardwareInfos)
    {
        public ChannelInfo ChannelInfo { get; private set; } = channelInfo;
        /// <summary>
        /// 所有的变量集合
        /// </summary>
        public List<VariableInfo> Variables { get; private set; } = variableInfos;
        /// <summary>
        /// 所有的硬件信息集合
        /// </summary>
        public List<HardwareInfo> HardwareInfos { get; private set; } = hardwareInfos;
    }
}
