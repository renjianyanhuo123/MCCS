using MCCS.Collecter.DllNative.Models;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MCCS.Collecter.HardwareDevices.BwController
{
    public sealed class MockControllerHardwareDevice : ControllerHardwareDeviceBase
    {
        private readonly IDisposable _acquisitionSubscription; 
        private readonly int _sampleRate;
        public MockControllerHardwareDevice(HardwareDeviceConfiguration configuration) : base(configuration)
        { 
            _sampleRate = configuration.Signals.Max(s => s.SampleRate); 
            _acquisitionSubscription = CreateAcquisitionLoop(); 
        }  

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
            var t = 1.0 / _sampleRate * 1.0;
            return Observable.Interval(TimeSpan.FromSeconds(t))
                .Where(_ => _isRunning && Status == HardwareConnectionStatus.Connected)
                .Subscribe(_ =>
                {
                    _dataSubject.OnNext(MockAcquireReading());
#if DEBUG
                    // Debug.WriteLine($"生成的模拟数据:{JsonConvert.SerializeObject(temp)}");
#endif
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataPoint MockAcquireReading()
        {
            // 模拟数据采集
            var rand = new Random();
            var res = new List<BatchCollectItemModel>();
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
            res.Add(StructDataToCollectModel(mockValue));
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
