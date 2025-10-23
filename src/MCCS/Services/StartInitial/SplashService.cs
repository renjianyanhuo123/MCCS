
using MCCS.Collecter.DllNative.Models;
using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.Services;
using Microsoft.Extensions.Configuration;
using StationSiteInfo = MCCS.Common.DataManagers.StationSites.StationSiteInfo;

namespace MCCS.Services.StartInitial
{
    public class SplashService : ISplashService
    {
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IControllerService _controllerService;
        private readonly IConfiguration _configuration;

        public SplashService(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository,
            IControllerService controllerService,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _controllerService = controllerService;
        }

        public async Task InitialHardwareDevicesAsync()
        {
            var currentUseStation = await _stationSiteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (currentUseStation?.StationSiteInfo == null) throw new ArgumentNullException("Current Use Station Site is Null");
            var deviceInfos = await _deviceInfoRepository.GetAllDevicesAsync();
            if (deviceInfos == null) throw new ArgumentNullException("Current Controllers is Null");
            var stationSiteInfo = new StationSiteInfo(currentUseStation.StationSiteInfo.Id, currentUseStation.StationSiteInfo.StationName);
            var allSignals = await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => c.IsDeleted == false); 
            var controllerInfos = deviceInfos
                .Where(c => c.DeviceType == DeviceTypeEnum.Controller)
                .Select(s =>
                {
                    var temp = new StationSiteControllerInfo(s.Id, s.DeviceName);
                    var bindSignals =
                        currentUseStation.Signals.Select(a =>
                        {
                            var temp1 = new StationSiteControllerSignalInfo(a.Id, a.BelongToControllerId, a.SignalName);
                            var bindDevice = deviceInfos.FirstOrDefault(c => c.Id == a.ConnectedDeviceId);
                            if (bindDevice != null) temp1.Link(new StationSiteDeviceInfo(bindDevice.Id, bindDevice.DeviceName));
                            return temp1;
                        }); 
                    temp.SignalInfos.AddRange(bindSignals);
                    return temp;
                }).ToList(); 
            // 创建所有的控制通道
            foreach (var ControlChannelSignalInfo in currentUseStation.ControlChannelSignalInfos)
            {
                var tempChannel = new StationSiteControlChannelInfo(ControlChannelSignalInfo.ControlChannelInfo.Id, ControlChannelSignalInfo.ControlChannelInfo.ChannelName, ControlChannelSignalInfo.ControlChannelInfo.ChannelType)
                {
                    BindSignals = ControlChannelSignalInfo.Signals.Select(s =>
                    {
                        var res = new StationSiteControllerSignalInfo(s.SignalInfo.Id, s.SignalInfo.BelongToControllerId,
                            s.SignalInfo.SignalName)
                        {
                            ControlChannelSignalType = s.SignalType
                        };
                        res.Link(new StationSiteDeviceInfo(s.LinkDeviceInfo?.Id ?? 0, s.LinkDeviceInfo?.DeviceName ?? ""));
                        return res;
                    }).ToList()
                };
                stationSiteInfo.ControlChannels.Add(tempChannel);
            }
            StartUpAllControllers(deviceInfos
                .Where(c => c.DeviceType == DeviceTypeEnum.Controller), allSignals);
            GlobalDataManager.Instance.SetValue(stationSiteInfo);
            GlobalDataManager.Instance.SetValue(controllerInfos);
        }


        /// <summary>
        /// 注册并启动所有控制器
        /// </summary>
        /// <param name="controllerInfos">所有的控制器设备</param>
        /// <param name="signals">所有的物理信号接口</param>
        /// <returns></returns>
        private void StartUpAllControllers(IEnumerable<DeviceInfo> controllerInfos,
            List<SignalInterfaceInfo> signals)
        {
            var isMock = Convert.ToBoolean(_configuration["AppSettings:IsMock"]);
            _controllerService.InitializeDll(isMock);
            var index = 0;
            foreach (var item in controllerInfos)
            {
                var configuration = new HardwareDeviceConfiguration
                {
                    DeviceId = item.Id,
                    DeviceAddressId = index++,
                    DeviceName = item.DeviceName,
                    DeviceType = item.DeviceType.ToString(),
                    IsSimulation = isMock,
                    ConnectionString = ""
                };
                configuration.Signals.AddRange(signals
                    .Where(c => c.BelongToControllerId == item.Id)
                    .Select(s => new HardwareSignalConfiguration
                    {
                        SignalId = s.Id,
                        SignalName = s.SignalName,
                        SignalAddress = (SignalAddressEnum)s.SignalAddress,
                        SignalType = SignalType.AnalogInput,
                        MinValue = s.DownLimitRange,
                        MaxValue = s.UpLimitRange
                    }));
                _controllerService.CreateController(configuration);
            }
            _controllerService.StartAllControllers();
        }
    }
}
