using MCCS.Collecter.DllNative.Models;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class MockControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly ReplaySubject<DataPoint> _dataSubject;
        private readonly IDisposable _acquisitionSubscription;

        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        {
            _dataSubject = new ReplaySubject<DataPoint>(bufferSize: 1000);
            _acquisitionSubscription = CreateAcquisitionLoop();
        } 

        public IObservable<DataPoint> DataStream => _dataSubject.AsObservable();

        public override bool ConnectToHardware()
        {
            Status = HardwareConnectionStatus.Connected;
            return true;
        }

        public override bool DisconnectFromHardware()
        {
            Status = HardwareConnectionStatus.Disconnected;
            return true;
        }

        private IDisposable CreateAcquisitionLoop()
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Where(_ => _isRunning && Status == HardwareConnectionStatus.Connected)
                .Subscribe(_ =>
                {
                    var temp = new DataPoint
                    {
                        DataQuality = DataQuality.Good,
                        DeviceId = DeviceId,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Value = MockAcquireReading()
                    };
                    _dataSubject.OnNext(temp);
#if DEBUG
                    Debug.WriteLine($"生成的模拟数据:{JsonConvert.SerializeObject(temp)}");
#endif
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint MockAcquireReading()
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<TNet_ADHInfo>();
            var mockValue = new TNet_ADHInfo
            {
                Net_AD_N =
                {
                    [0] = (float)(rand.NextDouble() * 100),
                    [1] = (float)(rand.NextDouble() * 100),
                    [2] = (float)(rand.NextDouble() * 100),
                    [3] = (float)(rand.NextDouble() * 100),
                    [4] = (float)(rand.NextDouble() * 100),
                    [5] = (float)(rand.NextDouble() * 100)
                },
                Net_AD_S =
                {
                    [0] = (float)(rand.NextDouble() * 100),
                    [1] = (float)(rand.NextDouble() * 100)
                }
            };
            res.Add(mockValue);
            return new DataPoint
            {
                DeviceId = DeviceId,
                Value = res,
                Timestamp = Stopwatch.GetTimestamp(),
                DataQuality = DataQuality.Good
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            _acquisitionSubscription.Dispose();
            _dataSubject?.OnCompleted();
            _dataSubject?.Dispose(); 
        }
    }
}
