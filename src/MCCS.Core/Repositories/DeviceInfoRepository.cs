using MCCS.Core.Models.Devices;

namespace MCCS.Core.Repositories
{
    public class DeviceInfoRepository(IFreeSql freeSql) : IDeviceInfoRepository
    {
        public async Task<List<DeviceInfo>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
        {
            return await freeSql
                .Select<DeviceInfo>()
                .Where(c => c.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        public async Task<DeviceInfo> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            return await freeSql
                .Select<DeviceInfo>()
                .Where(c => c.DeviceId == deviceId && c.IsDeleted == false)
                .FirstAsync(cancellationToken);
        }

        public async Task<bool> AddDeviceAsync(DeviceInfo device, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(device);
            var count = await freeSql.Insert(device).ExecuteAffrowsAsync(cancellationToken);
            return count >= 1;
        }
    }
}
