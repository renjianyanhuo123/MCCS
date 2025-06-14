using MCCS.Core.Devices.Commands;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices
{
    public interface IDevice : IDisposable
    {
        string Id { get; }
        string Name { get; }
        DeviceTypeEnum Type { get; }
        bool IsActive { get; }
        /// <summary>
        /// 数据流
        /// </summary>
        IObservable<DeviceData> DataStream { get; }

        /// <summary>
        /// 发送指令到设备
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<CommandResponse> SendCommandAsync(DeviceCommand command);

        /// <summary>
        /// 开始订阅数据流
        /// </summary>
        void Start();

        /// <summary>
        /// 停止订阅数据流
        /// </summary>
        void Stop();
        /// <summary>
        /// 设置采集频率
        /// </summary>
        /// <param name="interval"></param>
        void SetSamplingInterval(TimeSpan interval);
    }
}
