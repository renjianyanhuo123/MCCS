using MCCS.Core.Devices.Commands;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Devices
{
    public interface IDevice
    {
        string Id { get; }
        string Name { get; }
        DeviceTypeEnum Type { get; }
        /// <summary>
        /// 状态流 - 可观察的设备状态
        /// </summary>
        IObservable<DeviceStatusEnum> StatusStream { get; }
        /// <summary>
        /// 指令响应流 - 可观察的指令执行结果
        /// </summary>
        IObservable<CommandResponse> CommandResponseStream { get; }
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
        Task<DeviceStatusEnum> GetStatusAsync();

        /// <summary>
        /// 设备自己负责读取数据
        /// </summary>
        /// <returns></returns>
        Task<DeviceData> ReadDataAsync();

        /// <summary>
        /// 发送指令到设备
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<CommandResponse> SendCommandAsync(DeviceCommand command);
    }
}
