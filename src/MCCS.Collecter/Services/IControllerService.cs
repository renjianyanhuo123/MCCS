using MCCS.Collecter.HardwareDevices;

namespace MCCS.Collecter.Services
{
    public interface IControllerService
    {
        bool InitializeDll();

        bool CreateController(HardwareDeviceConfiguration configuration);

        bool RemoveController(int deviceId);
    }
}
