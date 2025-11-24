using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Models.Devices;
using System.Linq.Expressions;

namespace MCCS.Infrastructure.Repositories
{
    public class DeviceInfoRepository(IFreeSql<SystemDbFlag> freeSql) : IDeviceInfoRepository
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
                .Where(c => c.Id == deviceId && c.IsDeleted == false)
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

        public async Task<bool> UpdateSignalInfoAsync(SignalInterfaceInfo signalInterfaceInfo, CancellationToken cancellationToken = default)
        {
            var count = await freeSql.Update<SignalInterfaceInfo>()
                .Set(c => c.Address, signalInterfaceInfo.Address)
                .Set(c => c.SignalAddress, signalInterfaceInfo.SignalAddress)
                .Set(c => c.DataType, signalInterfaceInfo.DataType)
                .Set(c => c.SignalName, signalInterfaceInfo.SignalName)
                .Set(c => c.UpdateCycle, signalInterfaceInfo.UpdateCycle)
                .Set(c => c.WeightCoefficient, signalInterfaceInfo.WeightCoefficient)
                .Set(c => c.SignalRole,signalInterfaceInfo.SignalRole)
                .Set(c => c.UpLimitRange, signalInterfaceInfo.UpLimitRange)
                .Set(c => c.DownLimitRange, signalInterfaceInfo.DownLimitRange)
                .Set(c => c.ConnectedDeviceId, signalInterfaceInfo.ConnectedDeviceId)
                .Where(c => c.Id == signalInterfaceInfo.Id)
                .ExecuteAffrowsAsync(cancellationToken);
            return count > 0;
        }

        public async Task<long> AddSignalInfoAsync(SignalInterfaceInfo signalInterfaceInfo, CancellationToken cancellationToken = default)
        {
            return await freeSql.Insert(signalInterfaceInfo).ExecuteIdentityAsync(cancellationToken);
        }

        public async Task<bool> DeleteSignalInfoAsync(long signalId, CancellationToken cancellationToken = default)
        {
            var count = await freeSql.Update<SignalInterfaceInfo>()
                .Set(c => c.IsDeleted, true)
                .Where(c => c.Id == signalId)
                .ExecuteAffrowsAsync(cancellationToken);
            return count > 0;
        }

        public async Task<SignalInterfaceInfo> GetSignalInterfaceByIdAsync(long signalId, CancellationToken cancellationToken = default)
        {
            return await freeSql.Select<SignalInterfaceInfo>()
                .Where(c => c.Id == signalId && c.IsDeleted == false)
                .ToOneAsync(cancellationToken);
        }
    }
}
