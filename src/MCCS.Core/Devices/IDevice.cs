using MCCS.Core.Devices.Commands;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices
{
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// 设备ID - 唯一标识
        /// </summary>
        string Id { get; }
        /// <summary>
        /// 设备名称 - 用于描述设备
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 设备类型
        /// </summary>
        DeviceTypeEnum Type { get; }
        /// <summary>
        /// 设备状态
        /// </summary>
        DeviceStatusEnum Status { get; }
        /// <summary>
        /// 数据流
        /// </summary>
        IObservable<DeviceData> DataStream { get; }
        /// <summary>
        /// 指令状态流 - 用于接收指令执行结果
        /// </summary>
        IObservable<CommandResponse> CommandStatusStream { get; }
        /// <summary>
        /// 发送指令到设备
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<CommandResponse> SendCommandAsync(DeviceCommand command, CancellationToken cancellationToken = default);
        /// <summary>
        /// 开始订阅数据流
        /// </summary>
        void StartCollection();
        /// <summary>
        /// 停止订阅数据流
        /// </summary>
        void StopCollection();
    }
}
