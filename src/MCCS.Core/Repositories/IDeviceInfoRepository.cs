using MCCS.Core.Models.Devices;

namespace MCCS.Core.Repositories
{
    public interface IDeviceInfoRepository
    {
        Task<List<DeviceInfo>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

        Task<DeviceInfo> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default);

        Task<bool> AddDeviceAsync(DeviceInfo device, CancellationToken cancellationToken = default);
    }
}
