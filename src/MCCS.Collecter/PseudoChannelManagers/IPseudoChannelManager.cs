namespace MCCS.Collecter.PseudoChannelManagers
{
    public interface IPseudoChannelManager
    { 
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="configuations"></param>
        void Initialization(IEnumerable<PseudoChannelConfiguration> configuations);
        /// <summary>
        /// 获取虚拟通道
        /// </summary>
        /// <param name="pseudoChannelId"></param>
        /// <returns></returns>
        PseudoChannel GetPseudoChannelById(long pseudoChannelId);
        /// <summary>
        /// 获取所有的虚拟通道
        /// </summary>
        /// <returns></returns>
        IEnumerable<PseudoChannel> GetPseudoChannels();
    }
}
