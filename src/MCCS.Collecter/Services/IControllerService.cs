using MCCS.Collecter.HardwareDevices;
using MCCS.Infrastructure.TestModels;
using MCCS.Infrastructure.TestModels.ControlParams;

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

        bool StaticControl(long controllerId, StaticControlParams staticControlParam);

        bool DynamicControl(long controllerId, DynamicControlParams dynamicControlParam);

        bool RemoveController(int deviceId);

        void StartAllControllers();

        void StopAllControllers();
    }
}
