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
        private IDisposable _collectionSubscription;

        public string DeviceId => _device.Id;
        public IObservable<bool> IsCollecting { get; }

        // 数据流
        public IObservable<DeviceData> DataStream { get; }

        // 错误流
        public IObservable<Exception?> ErrorStream { get; }

        public DataCollector(
            IDevice device, 
            TimeSpan interval)
        {
            _device = device;

            // 创建共享的数据源
            var sharedDataSource = Observable
                .Interval(interval, NewThreadScheduler.Default)
                .TakeUntil(_stopSubject)
                .SelectMany(async _ => await _device.ReadDataAsync())
                .Publish()
                .RefCount();

            // 分离数据和错误
            DataStream = sharedDataSource
                .Retry() // 自动重试
                .Where(data => data != null);

            ErrorStream = sharedDataSource
                .Materialize()
                .Where(notification => notification.Kind == System.Reactive.NotificationKind.OnError)
                .Select(notification => notification.Exception);

            // 采集状态流
            var isCollectingSubject = new BehaviorSubject<bool>(false);
            IsCollecting = isCollectingSubject.AsObservable();

            // 监听订阅状态
            DataStream.Subscribe(
                _ => { },
                _ => isCollectingSubject.OnNext(false),
                () => isCollectingSubject.OnNext(false)
            );
        }

        public void Start()
        {
            if (_collectionSubscription != null) return;

            _collectionSubscription = DataStream.Subscribe();
        }

        public void Stop()
        {
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
