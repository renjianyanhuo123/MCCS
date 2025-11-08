using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public interface IControlChannelManager:IDisposable
    {
        /// <summary>
        /// 初始化控制通道
        /// </summary>
        /// <param name="configurations"></param>
        void Initialization(IEnumerable<ControlChannelConfiguration> configurations);

        /// <summary>
        /// 添加控制通道
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        bool AddControlChannel(ControlChannelConfiguration configuration);

        /// <summary>
        /// 移除控制通道
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        bool RemoveControlChannel(long channelId);

        /// <summary>
        /// 单个控制通道手动控制
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="speed">速度</param>
        /// <returns></returns>
        DeviceCommandContext ManualControl(long channelId, float speed);
        /// <summary>
        /// 单个控制通道静态控制
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="controlParam"></param>
        /// <returns></returns>
        DeviceCommandContext StaticControl(long channelId, StaticControlParams controlParam);
        /// <summary>
        /// 单个控制通道动态控制
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="dynamicControlParam"></param>
        /// <returns></returns>
        DeviceCommandContext DynamicControl(long channelId, DynamicControlParams dynamicControlParam); 
    }
}
