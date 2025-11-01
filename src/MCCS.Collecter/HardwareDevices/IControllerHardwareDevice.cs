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
        /// 当前命令执行状态
        /// </summary>
        CommandExecuteStatusEnum CurrentCommandStatus { get; }

        /// <summary>
        /// 命令状态变化流
        /// </summary>
        IObservable<CommandExecuteStatusEnum> CommandStatusStream { get; }
    }
}
