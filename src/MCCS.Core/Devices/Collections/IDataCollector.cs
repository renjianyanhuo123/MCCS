namespace MCCS.Core.Devices.Collections
{
    /// <summary>
    /// 数据采集接口
    /// </summary>
    public interface IDataCollector : IDisposable
    {
        /// <summary>
        /// 开启所有设备采集
        /// </summary>
        void StartCollection(TimeSpan? timeSpan = null);
        /// <summary>
        /// 停止所有设备采集
        /// </summary>
        void StopCollection();
        /// <summary>
        /// 获取指定设备的数据流
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        IObservable<DeviceData> GetDataStream(string deviceId);
        /// <summary>
        /// 获取所有数据流
        /// </summary>
        /// <returns></returns>
        IObservable<DeviceData> GetAllDataStreams();
        /// <summary>
        /// 汇集特定的数据流到总数据流中
        /// </summary>
        /// <param name="deviceId"></param>
        void SubscribeToDevice(string deviceId);
        /// <summary>
        /// 从总数据流中去除特定设备的数据流订阅
        /// </summary>
        /// <param name="deviceId"></param>
        void UnsubscribeFromDevice(string deviceId);
    }
}
