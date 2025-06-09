using FreeSql.Internal.ObjectPool;
using MCCS.Core.Collection;
using MCCS.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    public class DataAcquisitionService : IDataAcquisitionService, IDisposable
    {

        private readonly Subject<SensorData> _dataSubject;
        private readonly ConcurrentDictionary<string, SensorData> _latestData;
        private readonly IHardwareInterface _hardwareInterface;

        private CancellationTokenSource _acquisitionCts;
        private Task _acquisitionTask;
        private int _samplingRateHz = 100; // 默认100Hz
        private readonly object _stateLock = new();

        // 批量更新相关
        private readonly Timer _batchTimer;
        private readonly ConcurrentQueue<SensorData> _pendingUpdates;
        private const int BatchIntervalMs = 50; // 50ms批量更新一次

        public IObservable<SensorData> DataStream => _dataSubject.AsObservable();
        public bool IsAcquiring { get; private set; }
        public event EventHandler<BatchDataUpdateEventArgs> BatchDataUpdated; 

        public DataAcquisitionService(
             IHardwareInterface hardwareInterface)
        {
            _hardwareInterface = hardwareInterface;
            // _logger = logger;
            _dataSubject = new Subject<SensorData>();
            _latestData = new ConcurrentDictionary<string, SensorData>();
            _pendingUpdates = new ConcurrentQueue<SensorData>();

            // 初始化批量更新定时器
            _batchTimer = new Timer(ProcessBatchUpdates, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task StartAcquisitionAsync(CancellationToken cancellationToken = default)
        {
            lock (_stateLock)
            {
                if (IsAcquiring)
                {
                    // _logger.LogWarning("数据采集已经在进行中");
                    return;
                }
                IsAcquiring = true;
            }

            _acquisitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // 启动批量更新定时器
            _batchTimer.Change(BatchIntervalMs, BatchIntervalMs);

            _acquisitionTask = Task.Run(() => AcquisitionLoop(_acquisitionCts.Token), _acquisitionCts.Token);

            // _logger.LogInformation($"数据采集已启动，采样率: {_samplingRateHz}Hz");

            await Task.CompletedTask;
        }

        public async Task StopAcquisitionAsync()
        {
            lock (_stateLock)
            {
                if (!IsAcquiring)
                {
                    return;
                }
                IsAcquiring = false;
            }

            _batchTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _acquisitionCts?.Cancel();

            try
            {
                if (_acquisitionTask != null)
                {
                    await _acquisitionTask;
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }

            // _logger.LogInformation("数据采集已停止");
        }

        public void SetSamplingRate(int samplingRateHz)
        {
            if (samplingRateHz < 1 || samplingRateHz > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(samplingRateHz), "采样率必须在1-1000Hz之间");
            }

            _samplingRateHz = samplingRateHz;
            // _logger.LogInformation($"采样率已设置为: {samplingRateHz}Hz");
        }

        public SensorData? GetLatestData(string actuatorId)
        {
            return _latestData.TryGetValue(actuatorId, out SensorData? data) ? data : null;
        }

        private async Task AcquisitionLoop(CancellationToken cancellationToken)
        {
            var samplingInterval = TimeSpan.FromMilliseconds(1000.0 / _samplingRateHz);
            var nextSampleTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 从硬件读取数据
                    var hardwareData = await _hardwareInterface.ReadSensorDataAsync(cancellationToken);

                    foreach (var data in hardwareData)
                    {
                        var sensorData = new SensorData
                        {
                            ActuatorId = data.ActuatorId,
                            Force = data.ForceValue,
                            Displacement = data.DisplacementValue,
                            Timestamp = DateTime.UtcNow,
                            IsValid = data.IsValid
                        };

                        // 更新最新数据缓存
                        _latestData.AddOrUpdate(sensorData.ActuatorId, sensorData, (key, old) => sensorData);

                        // 发布到响应式流
                        _dataSubject.OnNext(sensorData);

                        // 加入批量更新队列
                        _pendingUpdates.Enqueue(sensorData);
                    }

                    // 精确定时
                    nextSampleTime = nextSampleTime.Add(samplingInterval);
                    var delay = nextSampleTime - DateTime.UtcNow;

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    else
                    {
                        // 如果延迟为负，说明采集速度跟不上设定频率
                        // _logger.LogWarning($"数据采集延迟: {-delay.TotalMilliseconds:F2}ms");
                        nextSampleTime = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "数据采集循环中发生错误");
                    await Task.Delay(100, cancellationToken); // 错误后短暂延迟
                }
            }
        }

        private void ProcessBatchUpdates(object state)
        {
            var updates = new Dictionary<string, SensorData>();

            while (_pendingUpdates.TryDequeue(out var data))
            {
                updates[data.ActuatorId] = data;
            }

            if (updates.Count > 0)
            {
                BatchDataUpdated?.Invoke(this, new BatchDataUpdateEventArgs
                {
                    UpdatedData = updates,
                    BatchTimestamp = DateTime.UtcNow
                });
            }
        }

        public void Dispose()
        {
            StopAcquisitionAsync().Wait(TimeSpan.FromSeconds(5));
            _batchTimer?.Dispose();
            _dataSubject?.Dispose();
            _acquisitionCts?.Dispose();
        }
    }
}
