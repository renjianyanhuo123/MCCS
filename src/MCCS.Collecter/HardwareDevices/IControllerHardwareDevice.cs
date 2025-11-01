using MCCS.Infrastructure.TestModels.Commands;

namespace MCCS.Collecter.HardwareDevices
{
    public interface IControllerHardwareDevice : IDisposable
    {
        bool ConnectToHardware();
        bool DisconnectFromHardware();
        void StartDataAcquisition();
        void StopDataAcquisition();

        /// <summary>
        /// 获取指定设备的命令执行状态
        /// </summary>
        /// <param name="deviceId">设备ID（通道ID）</param>
        /// <returns>命令执行状态</returns>
        CommandExecuteStatusEnum GetDeviceCommandStatus(long deviceId);

        /// <summary>
        /// 命令状态变化流（包含设备ID信息）
        /// </summary>
        IObservable<CommandStatusChangeEvent> CommandStatusStream { get; }
    }
}
