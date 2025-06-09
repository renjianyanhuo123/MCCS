using MCCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    public interface IDataAcquisitionService
    {
        /// <summary>
        /// 获取实时数据流
        /// </summary>
        IObservable<SensorData> DataStream { get; }

        /// <summary>
        /// 批量数据更新事件
        /// </summary>
        event EventHandler<BatchDataUpdateEventArgs> BatchDataUpdated;

        /// <summary>
        /// 开始数据采集
        /// </summary>
        Task StartAcquisitionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止数据采集
        /// </summary>
        Task StopAcquisitionAsync();

        /// <summary>
        /// 获取当前采集状态
        /// </summary>
        bool IsAcquiring { get; }

        /// <summary>
        /// 设置采集频率（Hz）
        /// </summary>
        void SetSamplingRate(int samplingRateHz);

        /// <summary>
        /// 获取特定执行器的最新数据
        /// </summary>
        SensorData GetLatestData(string actuatorId);
    }
}
