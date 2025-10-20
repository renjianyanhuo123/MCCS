using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.Services
{
    public interface IControllerService : IDisposable
    {
        bool InitializeDll(bool isMock = false);

        ControllerHardwareDeviceBase GetControllerInfo(long controllerId);

        bool CreateController(HardwareDeviceConfiguration configuration);

        bool RemoveController(int deviceId);

        void StartAllControllers();

        void StopAllControllers();
    }
}
