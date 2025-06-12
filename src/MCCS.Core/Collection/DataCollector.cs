using MCCS.Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace MCCS.Core.Collection
{
    public class DataCollector : IDataCollector
    {
        private readonly IDevice _device;
        private readonly Subject<Unit> _stopSubject = new();
        private readonly BehaviorSubject<bool> _isCollectingSubject = new(false);
        private IDisposable _collectionSubscription;

        public string DeviceId => _device.Id;
        public bool IsCollecting => _isCollectingSubject.Value;

        /// <summary>
        /// 数据流
        /// </summary>
        public IObservable<DeviceData> DataStream { get; }

        /// <summary>
        /// 错误流
        /// </summary>
        // public IObservable<Exception?> ErrorStream { get; }

        public DataCollector(
            IDevice device, 
            TimeSpan interval)
        {
            _device = device;

            DataStream = Observable
                .Interval(interval, NewThreadScheduler.Default)
                .TakeUntil(_stopSubject)
                .SelectMany(async _ => await _device.ReadDataAsync())
                .Where(data => data != null)
                .Do(_ => { },
                    _ => _isCollectingSubject.OnNext(false),
                    () => _isCollectingSubject.OnNext(false))
                .Publish()
                .RefCount();
        }

        public void Start()
        {
            if (_collectionSubscription != null) return;

            _collectionSubscription = DataStream.Subscribe(
                data => { /* 数据处理在外部订阅中完成 */ },
                error => _isCollectingSubject.OnNext(false),
                () => _isCollectingSubject.OnNext(false));
        }

        public void Stop()
        {
            _isCollectingSubject.OnNext(false);
            _stopSubject.OnNext(Unit.Default);
            _collectionSubscription?.Dispose();
            _collectionSubscription = null;
        }

        public void Dispose()
        {
            Stop();
            _stopSubject.Dispose();
        }
    }
}
