using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;
using MCCS.Infrastructure.TestModels.CommandTracking;

namespace MCCS.Collecter.Services
{
    public interface IControllerService : IDisposable
    {
        bool InitializeDll(bool isMock = false);

        ControllerHardwareDeviceBase GetControllerInfo(long controllerId);

        bool CreateController(HardwareDeviceConfiguration configuration);

        bool OperationSigngleValve(long controllerId, bool isOpen);

        bool OperationTest(bool isStart);

        bool OperationControlMode(long controllerId, SystemControlState controlMode);

        bool ManualControl(long controllerId, long deviceId, float speed);

        /// <summary>
        /// 静态控制（带命令跟踪）
        /// </summary>
        /// <returns>命令记录，如果创建失败则返回null</returns>
        CommandRecord? StaticControl(long controllerId, long deviceId, StaticControlParams staticControlParam);

        /// <summary>
        /// 动态控制（带命令跟踪）
        /// </summary>
        /// <returns>命令记录，如果创建失败则返回null</returns>
        CommandRecord? DynamicControl(long controllerId, long deviceId, DynamicControlParams dynamicControlParam);

        bool RemoveController(int deviceId);

        void StartAllControllers();

        void StopAllControllers();
    }
}
