using MCCS.Station.Abstractions.Models;

namespace MCCS.Station.Abstractions.Interfaces
{
    /// <summary>
    /// 用于加载/配置
    /// </summary>
    public interface IStationRuntime
    {
        /// <summary>
        /// 是否存在当前站点的Profile文件
        /// </summary>
        /// <returns></returns>
        bool IsExistCurrentStationProfile();
        /// <summary>
        /// 构建当前使用的站点Profile文件
        /// </summary>
        /// <param name="stationSiteInfo"></param>
        void BuildCurrentStationProfile(StationSiteInfo stationSiteInfo);
        /// <summary>
        /// 构建当前使用的站点Profile文件（异步）
        /// </summary>
        /// <param name="stationSiteInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BuildCurrentStationProfileAsync(StationSiteInfo stationSiteInfo, CancellationToken cancellationToken = default);
        ///// <summary>
        ///// 根据Profile文件获取当前的需要运行的站点
        ///// </summary>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        // Task<StationSiteInfo?> GetStationInfoBtProfileAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 初始化站点(Host中使用)
        /// </summary>
        /// <param name="isMock"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<StationSiteInfo> InitialStationSiteAsync(bool isMock, CancellationToken cancellationToken = default);
    }
}
