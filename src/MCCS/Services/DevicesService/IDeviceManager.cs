using MCCS.Core.Devices;
using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Manager;
using MCCS.Core.Models.Devices;

namespace MCCS.Services.DevicesService
{
    public interface IDeviceManager
    {
        /// <summary>
        /// 公开的事件流
        /// </summary>
        public IObservable<DeviceEvent> DeviceEvents { get; }

        /// <summary>
        /// 设备状态变化流
        /// </summary>
        public IObservable<DeviceStatusEvent> StatusChanges { get; }
        /// <summary>
        /// 设备添加/移除流
        /// </summary>
        public IObservable<DeviceRegistrationEvent> RegistrationChanges { get; }
        /// <summary>
        /// 指令执行流(合并)
        /// </summary>
        public IObservable<CommandResponse> CommandExecutions { get; }

        /// <summary>
        /// 所有设备的状态聚合流
        /// </summary>
        public IObservable<Dictionary<string, DeviceStatusEnum>> AllDeviceStatuses { get; }
        /// <summary>
        ///  注册设备
        /// </summary>
        /// <param name="device"></param>
        public void RegisterDevice(IDevice device);
        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="deviceId"></param>
        public Task UnregisterDevice(string deviceId);

        /// <summary>
        /// 从数据库总注册所有设备
        /// </summary>
        /// <returns></returns>
        public Task<bool> RegisterAllDeviceFromRepository();

        public IDevice? GetDevice(string deviceId);

        public IObservable<IDevice?> GetDeviceStream(string deviceId);

        public IObservable<IEnumerable<IDevice>> GetAllDevicesStream();

        /// <summary>
        /// 给某台设备发送指令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task<CommandResponse> SendCommandAsync(DeviceCommand command);
        /// <summary>
        /// 给多台设备发送指令
        /// 批量操作，需要实时看到每个指令的执行进度;可能有长时间运行的指令序列;需要流式处理结果
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public IObservable<CommandResponse> SendCommandsAsync(IEnumerable<DeviceCommand> commands)
    }
}
