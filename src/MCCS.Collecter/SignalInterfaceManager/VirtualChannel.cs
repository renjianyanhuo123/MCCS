using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Data;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 虚拟通道 - 组合多个物理信号进行计算输出
    /// </summary>
    public sealed class VirtualChannel : IDisposable
    {
        private readonly Subject<SignalData> _dataSubject;
        private readonly Dictionary<long, double> _latestValues;
        private readonly List<IDisposable> _subscriptions;
        private bool _isRunning;

        public VirtualChannel(
            long channelId,
            string channelName,
            string formula,
            List<long> bindedSignalIds,
            double rangeMin = 0,
            double rangeMax = 100)
        {
            ChannelId = channelId;
            ChannelName = channelName;
            Formula = formula;
            BindedSignalIds = bindedSignalIds;
            RangeMin = rangeMin;
            RangeMax = rangeMax;

            _dataSubject = new Subject<SignalData>();
            _latestValues = new Dictionary<long, double>();
            _subscriptions = new List<IDisposable>();
            DataStream = _dataSubject.AsObservable();
        }

        public long ChannelId { get; }
        public string ChannelName { get; }
        public string Formula { get; }
        public List<long> BindedSignalIds { get; }
        public double RangeMin { get; }
        public double RangeMax { get; }

        /// <summary>
        /// 虚拟通道输出数据流
        /// </summary>
        public IObservable<SignalData> DataStream { get; }

        /// <summary>
        /// 启动虚拟通道，订阅物理信号的数据流
        /// </summary>
        /// <param name="physicalSignals">物理信号通道字典</param>
        public void Start(Dictionary<long, HardwareSignalChannel> physicalSignals)
        {
            if (_isRunning) return;

            // 订阅所有绑定的物理信号
            foreach (var signalId in BindedSignalIds)
            {
                if (!physicalSignals.TryGetValue(signalId, out var signal))
                    continue;

                var subscription = signal.DataStream.Subscribe(
                    data =>
                    {
                        lock (_latestValues)
                        {
                            _latestValues[signalId] = data.Value;

                            // 当所有信号都有数据时，计算输出
                            if (_latestValues.Count >= BindedSignalIds.Count)
                            {
                                var result = CalculateFormula();
                                if (result.HasValue)
                                {
                                    _dataSubject.OnNext(new SignalData
                                    {
                                        SignalId = ChannelId,
                                        Value = result.Value,
                                        Timestamp = data.Timestamp,
                                        IsValid = true
                                    });
                                }
                            }
                        }
                    },
                    error => { /* 错误处理暂时忽略 */ }
                );

                _subscriptions.Add(subscription);
            }

            _isRunning = true;
        }

        /// <summary>
        /// 停止虚拟通道
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
            _latestValues.Clear();
            _isRunning = false;
        }

        /// <summary>
        /// 根据公式计算结果
        /// </summary>
        private double? CalculateFormula()
        {
            if (string.IsNullOrWhiteSpace(Formula))
                return null;

            try
            {
                // 替换公式中的信号ID为实际值
                var expression = Formula;
                foreach (var kvp in _latestValues)
                {
                    // 支持 {SignalId} 或 ${SignalId} 的格式
                    expression = expression.Replace($"{{{kvp.Key}}}", kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    expression = expression.Replace($"${{{kvp.Key}}}", kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    expression = expression.Replace($"[{kvp.Key}]", kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                // 使用 DataTable.Compute 计算表达式（简单但有效的方式）
                using var dt = new DataTable();
                var result = dt.Compute(expression, null);

                if (result != null && double.TryParse(result.ToString(), out var value))
                {
                    // 限制在范围内
                    return Math.Clamp(value, RangeMin, RangeMax);
                }
            }
            catch
            {
                // 计算失败，返回null
            }

            return null;
        }

        public void Dispose()
        {
            Stop();
            _dataSubject.OnCompleted();
            _dataSubject.Dispose();
        }
    }
}
