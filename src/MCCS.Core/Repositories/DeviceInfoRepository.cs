using MCCS.Core.Models.Devices;
using System.Linq.Expressions;

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

        public async Task<bool> AddDeviceAsync(DeviceInfo device, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(device);
            var count = await freeSql.Insert(device).ExecuteAffrowsAsync(cancellationToken);
            return count >= 1;
        }

        public async Task<List<DeviceInfo>> GetDevicesByExpressionAsync(Expression<Func<DeviceInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<DeviceInfo>().Where(expression).ToListAsync(cancellationToken);
        }

        public List<DeviceInfo> GetDevicesByExpression(Expression<Func<DeviceInfo, bool>> expression)
        {
            return freeSql.Select<DeviceInfo>().Where(expression).ToList();
        }

        public async Task<DeviceInfo> GetDeviceByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            return await freeSql
                .Select<DeviceInfo>()
                .Where(c => c.DeviceId == deviceId && c.IsDeleted == false)
                .FirstAsync(cancellationToken);
        }

        public async Task<DeviceInfo> GetDeviceByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await freeSql
                .Select<DeviceInfo>()
                .Where(c => c.Id == id)
                .FirstAsync(cancellationToken);
        }
    }
}
