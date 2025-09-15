
using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using System.Collections.Generic;
using MCCS.Collecter.HardwareDevices.BwController;
using MCCS.Collecter.HardwareDevices;

namespace MCCS.Services.StartInitial
{
    public class SplashService : ISplashService
    {
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        public SplashService(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
        }

        public async Task InitialHardwareDevicesAsync()
        {
            var currentUseStation = await _stationSiteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (currentUseStation?.StationSiteInfo == null) throw new ArgumentNullException("Current Use Station Site is Null");
            var deviceInfos =
                await _deviceInfoRepository.GetAllDevicesAsync();
            if (deviceInfos == null) throw new ArgumentNullException("Current Controllers is Null");
            var stationSiteInfo = new StationSiteInfo(currentUseStation.StationSiteInfo.Id,
                currentUseStation.StationSiteInfo.StationName);
            var controllerInfos = deviceInfos
                .Where(c => c.DeviceType == DeviceTypeEnum.Controller)
                .Select(s =>
                {
                    var temp = new StationSiteControllerInfo(s.Id, s.DeviceName);
                    var bindSignals =
                        currentUseStation.Signals.Select(a =>
                        {
                            var temp1 = new StationSiteControllerSignalInfo(a.Id, a.SignalName);
                            var bindDevice = deviceInfos.FirstOrDefault(c => c.Id == a.ConnectedDeviceId);
                            if (bindDevice != null) temp1.Link(new StationSiteDeviceInfo(bindDevice.Id, bindDevice.DeviceName));
                            return temp1;
                        }); 
                    temp.SignalInfos.AddRange(bindSignals);
                    return temp;
                });
            stationSiteInfo.ControllerInfos.AddRange(controllerInfos);
            await StartUpAllControllers(deviceInfos
                .Where(c => c.DeviceType == DeviceTypeEnum.Controller));
            GlobalDataManager.Instance.SetValue(stationSiteInfo);
        }
        /// <summary>
        /// 启动所有控制器
        /// </summary>
        /// <param name="controllerInfos"></param>
        /// <returns></returns>
        private async Task StartUpAllControllers(IEnumerable<DeviceInfo> controllerInfos)
        {
            var signals = await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => c.IsDeleted == false);
            var hardwareDevices = new List<IControllerHardwareDevice>();
            foreach (var item in controllerInfos)
            {
                var configuration = new HardwareDeviceConfiguration
                {
                    DeviceId = item.Id,
                    DeviceName = item.DeviceName,
                    DeviceType = item.DeviceType.ToString(),
                    ConnectionString = ""
                };
                configuration.Signals.AddRange(signals
                    .Where(c => c.BelongToControllerId == item.Id)
                    .Select(s => new HardwareSignalConfiguration
                    {
                        SignalId = s.Id.ToString(),
                        SignalName = s.SignalName,
                        SignalType = SignalType.AnalogInput,
                        MinValue = s.DownLimitRange,
                        MaxValue = s.UpLimitRange
                    }));
                var controller = new BwControllerHardwareDevice(configuration);
                hardwareDevices.Add(controller);
            }
            GlobalDataManager.Instance.SetValue(hardwareDevices);
        }
    }
}
