using MCCS.Core.Collection;
using MCCS.Core.Devices;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Models.Devices;
using MCCS.Services.DevicesService;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    /// <summary>
    /// 数据采集管理器
    /// </summary>
    public class DataAcquisitionManager : IDataAcquisitionManager
    {
        private readonly Dictionary<string, DataCollector> _collectors = new();
        private readonly DeviceManager _deviceManager;
        private readonly Subject<DeviceData> _allDataSubject = new();
        private readonly Subject<DataCollectionError> _errorSubject = new();

        // 所有设备的数据流
        public IObservable<DeviceData> AllDataStream => _allDataSubject.AsObservable();
        // 错误流
        public IObservable<DataCollectionError> ErrorStream => _errorSubject.AsObservable();

        // 特定设备的数据流
        public IObservable<DeviceData> GetDeviceDataStream(string deviceId)
        {
            return AllDataStream.Where(data => data.DeviceId == deviceId);
        }

        // 数据统计流（每秒更新）
        public IObservable<DataStatistics> StatisticsStream { get; }

        public DataAcquisitionManager(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;

            // 监听设备状态变化，自动管理采集器
            _deviceManager.StatusChanges
            .Where(e => e.NewStatus == DeviceStatusEnum.Disconnected || e.NewStatus == DeviceStatusEnum.Error)
                .Subscribe(e => StopCollection(e.DeviceId));

            // 创建统计流
            StatisticsStream = AllDataStream
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(dataList => new DataStatistics
                {
                    Timestamp = DateTime.Now,
                    DeviceCount = _collectors.Count,
                    DataPointsPerSecond = dataList.Count,
                    ActiveDevices = dataList.Select(d => d.DeviceId).Distinct().Count()
                })
                .Publish()
                .RefCount();
        }

        public void StartCollection(string deviceId, TimeSpan? interval = null)
        {
            var device = _deviceManager.GetDevice(deviceId);
            if (device == null)
                throw new InvalidOperationException($"Device {deviceId} not found");

            if (_collectors.ContainsKey(deviceId))
                return;

            var collector = new DataCollector(device, interval ?? TimeSpan.FromSeconds(1));

            // 订阅数据流
            collector.DataStream.Subscribe(data => _allDataSubject.OnNext(data));

            // 订阅错误流
            collector.ErrorStream.Subscribe(error =>
                _errorSubject.OnNext(new DataCollectionError
                {
                    DeviceId = deviceId,
                    Error = error,
                    Timestamp = DateTime.Now
                }));

            _collectors[deviceId] = collector;
            collector.Start();
        }

        public void StopCollection(string deviceId)
        {
            if (_collectors.TryGetValue(deviceId, out var collector))
            {
                collector.Stop();
                collector.Dispose();
                _collectors.Remove(deviceId);
            }
        }

        /// <summary>
        /// 创建数据处理管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipelineFactory"></param>
        /// <returns></returns>
        public IObservable<T> CreateDataPipeline<T>(
            Func<IObservable<DeviceData>, IObservable<T>> pipelineFactory)
        {
            return pipelineFactory(AllDataStream);
        }

        public void Dispose()
        {
            foreach (var collector in _collectors.Values)
            {
                collector.Dispose();
            }
            _collectors.Clear();
            _allDataSubject.Dispose();
            _errorSubject.Dispose();
        }
    }
}
