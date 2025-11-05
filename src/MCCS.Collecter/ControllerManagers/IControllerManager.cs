using MCCS.Collecter.ControllerManagers.Entities;
using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;

namespace MCCS.Collecter.ControllerManagers
{
    public interface IControllerManager : IDisposable
    {
        /// <summary>
        /// 根据信号ID获取控制状态
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        SystemControlState GetControlStateBySignalId(long signalId);

        bool InitializeDll(bool isMock = false);

        ControllerHardwareDeviceBase GetControllerInfo(long controllerId);

        bool CreateController(HardwareDeviceConfiguration configuration);

        bool OperationSigngleValve(long controllerId, bool isOpen);

        bool OperationTest(bool isStart);

        bool OperationControlMode(long controllerId, SystemControlState controlMode); 

        bool RemoveController(int deviceId);

        void StartAllControllers();

        void StopAllControllers();
    }
}
