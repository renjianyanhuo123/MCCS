using MCCS.Core.Models.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Manager
{
    public interface IDeviceManager : IDisposable
    {
        /// <summary>
        /// 注册设备---从数据库加载
        /// </summary>
        /// <returns></returns>
        Task<bool> RegisterDeviceFromRepository();
        /// <summary>
        /// 注册设备
        /// </summary>
        /// <param name="deviceInfo"></param>
        void RegisterDevice(DeviceInfo deviceInfo);
        /// <summary>
        /// 批量注册设备
        /// </summary>
        /// <param name="deviceInfo"></param>
        void RegisterDevices(IEnumerable<DeviceInfo> deviceInfo);
        /// <summary>
        /// 获取某个设备
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        IDevice? GetDevice(string deviceId);
        /// <summary>
        /// 开始所有设备
        /// </summary>
        void StartAllDevices();
        /// <summary>
        /// 停止所有设备
        /// </summary>
        void StopAllDevices();
        /// <summary>
        /// 开始某个设备
        /// </summary>
        /// <param name="deviceId"></param>
        void StartDevice(string deviceId);
        /// <summary>
        /// 停止某个设备
        /// </summary>
        /// <param name="deviceId"></param>
        void StopDevice(string deviceId);
    }
}
