using MCCS.Infrastructure.Domain.StationSites;
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
        /// <summary>
        /// 映射StationInfo模型
        /// </summary>
        /// <param name="aggregateInfo"></param>
        /// <returns></returns>
        StationSiteInfo MappingStationSiteInfo(StationSiteAggregate aggregateInfo);
    }
}
