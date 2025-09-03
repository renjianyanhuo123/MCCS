using System.Linq.Expressions;
using MCCS.Core.Models.Devices;

namespace MCCS.Core.Repositories
{
    public interface IDeviceInfoRepository
    {
        Task<List<DeviceInfo>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

        Task<List<DeviceInfo>> GetDevicesByExpressionAsync(Expression<Func<DeviceInfo, bool>> expression, CancellationToken cancellationToken = default);

        List<DeviceInfo> GetDevicesByExpression(Expression<Func<DeviceInfo, bool>> expression);

        Task<DeviceInfo> GetDeviceByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);

        Task<DeviceInfo> GetDeviceByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<long> AddDeviceAsync(DeviceInfo device, CancellationToken cancellationToken = default);

        Task<List<SignalInterfaceInfo>> GetSignalInterfacesByExpressionAsync(Expression<Func<SignalInterfaceInfo, bool>> expression, CancellationToken cancellationToken = default);

        Task<bool> DeleteDeviceInfoAsync(long deviceId, CancellationToken cancellationToken = default);

        Task<bool> UpdateDeviceInfoAsync(DeviceInfo device, CancellationToken cancellationToken = default);
    }
}
