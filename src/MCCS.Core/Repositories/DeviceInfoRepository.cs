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

        public async Task<long> AddDeviceAsync(DeviceInfo device, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(device);
            var addId = await freeSql.Insert(device).ExecuteIdentityAsync(cancellationToken);
            return addId;
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

        public async Task<List<SignalInterfaceInfo>> GetSignalInterfacesByExpressionAsync(Expression<Func<SignalInterfaceInfo, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<SignalInterfaceInfo>()
                .Where(expression)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> DeleteDeviceInfoAsync(long deviceId, CancellationToken cancellationToken = default)
        {
            var count = await freeSql.Update<DeviceInfo>()
                .Set(c => c.IsDeleted, true)
                .Where(c => c.Id == deviceId)
                .ExecuteAffrowsAsync(cancellationToken);
            return count > 0;
        }

        public async Task<bool> UpdateDeviceInfoAsync(DeviceInfo device, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(device);
            var count = await freeSql.Update<DeviceInfo>()
                .Set(c => c.DeviceName, device.DeviceName)
                .Set(c => c.Description, device.Description)
                .Set(c => c.DeviceType, device.DeviceType)
                .Set(c => c.FunctionType, device.FunctionType)
                .Where(c => c.Id == device.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            return count > 0;
        }
    }
}
