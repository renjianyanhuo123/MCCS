using MCCS.Core.Models.StationSites;
using MCCS.Collecter.HardwareDevices;
using System.Reactive.Linq;

namespace MCCS.Collecter.SignalInterfaceManager
{
    /// <summary>
    /// 控制通道 - 实现闭环控制逻辑
    /// </summary>
    public sealed class ControlChannel : IDisposable
    {
        private readonly List<IDisposable> _subscriptions;
        private IControllerHardwareDevice? _controlDevice;
        private bool _isRunning;

        // 反馈信号的最新值
        private double? _latestPositionValue;
        private double? _latestForceValue;

        // 控制参数
        private double _setpoint;
        private short _outputLimitation;

        public ControlChannel(
            long channelId,
            string channelName,
            ChannelTypeEnum channelType,
            ControlChannelModeTypeEnum controlMode,
            double controlCycle,
            short outputLimitation = 100)
        {
            ChannelId = channelId;
            ChannelName = channelName;
            ChannelType = channelType;
            ControlMode = controlMode;
            ControlCycle = controlCycle;
            _outputLimitation = outputLimitation;

            _subscriptions = new List<IDisposable>();

            PositionFeedbackSignalId = null;
            ForceFeedbackSignalId = null;
            OutputSignalId = null;
        }

        public long ChannelId { get; }
        public string ChannelName { get; }
        public ChannelTypeEnum ChannelType { get; }
        public ControlChannelModeTypeEnum ControlMode { get; }
        public double ControlCycle { get; }

        /// <summary>
        /// 位置反馈信号ID
        /// </summary>
        public long? PositionFeedbackSignalId { get; private set; }

        /// <summary>
        /// 力反馈信号ID
        /// </summary>
        public long? ForceFeedbackSignalId { get; private set; }

        /// <summary>
        /// 输出信号ID
        /// </summary>
        public long? OutputSignalId { get; private set; }

        /// <summary>
        /// 绑定反馈信号
        /// </summary>
        public void BindFeedbackSignals(long? positionSignalId, long? forceSignalId, long? outputSignalId)
        {
            PositionFeedbackSignalId = positionSignalId;
            ForceFeedbackSignalId = forceSignalId;
            OutputSignalId = outputSignalId;
        }

        /// <summary>
        /// 启动控制通道
        /// </summary>
        /// <param name="physicalSignals">物理信号字典</param>
        /// <param name="virtualChannels">虚拟通道字典（可选）</param>
        /// <param name="controlDevice">控制设备</param>
        public void Start(
            Dictionary<long, HardwareSignalChannel> physicalSignals,
            Dictionary<long, VirtualChannel>? virtualChannels,
            IControllerHardwareDevice controlDevice)
        {
            if (_isRunning) return;

            _controlDevice = controlDevice;

            // 订阅位置反馈信号
            if (PositionFeedbackSignalId.HasValue)
            {
                var signal = GetSignalDataStream(PositionFeedbackSignalId.Value, physicalSignals, virtualChannels);
                if (signal != null)
                {
                    var subscription = signal.Subscribe(
                        data => _latestPositionValue = data.Value,
                        error => { /* 错误处理暂时忽略 */ }
                    );
                    _subscriptions.Add(subscription);
                }
            }

            // 订阅力反馈信号
            if (ForceFeedbackSignalId.HasValue)
            {
                var signal = GetSignalDataStream(ForceFeedbackSignalId.Value, physicalSignals, virtualChannels);
                if (signal != null)
                {
                    var subscription = signal.Subscribe(
                        data => _latestForceValue = data.Value,
                        error => { /* 错误处理暂时忽略 */ }
                    );
                    _subscriptions.Add(subscription);
                }
            }

            _isRunning = true;
        }

        /// <summary>
        /// 停止控制通道
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();

            _latestPositionValue = null;
            _latestForceValue = null;
            _isRunning = false;
        }

        /// <summary>
        /// 设置控制目标值
        /// </summary>
        public void SetSetpoint(double setpoint)
        {
            _setpoint = setpoint;
        }

        /// <summary>
        /// 获取当前反馈值
        /// </summary>
        public double? GetCurrentFeedback()
        {
            return ChannelType switch
            {
                ChannelTypeEnum.Position => _latestPositionValue,
                ChannelTypeEnum.Force => _latestForceValue,
                _ => null
            };
        }

        /// <summary>
        /// 执行控制（由外部控制循环调用）
        /// 这是一个简化版本，实际的PID控制逻辑应该在这里实现
        /// </summary>
        public double? CalculateControlOutput()
        {
            if (!_isRunning || _controlDevice == null)
                return null;

            var feedback = GetCurrentFeedback();
            if (!feedback.HasValue)
                return null;

            // 简化的控制逻辑：计算误差
            var error = _setpoint - feedback.Value;

            // 这里应该实现完整的PID控制算法
            // 暂时返回简单的比例控制
            double output = error;

            // 应用输出限制
            var limit = _outputLimitation / 100.0;
            output = Math.Clamp(output, -limit, limit);

            return output;
        }

        /// <summary>
        /// 从物理信号或虚拟通道获取数据流
        /// </summary>
        private IObservable<SignalData>? GetSignalDataStream(
            long signalId,
            Dictionary<long, HardwareSignalChannel> physicalSignals,
            Dictionary<long, VirtualChannel>? virtualChannels)
        {
            // 先从物理信号查找
            if (physicalSignals.TryGetValue(signalId, out var physicalSignal))
            {
                return physicalSignal.DataStream;
            }

            // 再从虚拟通道查找
            if (virtualChannels != null && virtualChannels.TryGetValue(signalId, out var virtualChannel))
            {
                return virtualChannel.DataStream;
            }

            return null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
