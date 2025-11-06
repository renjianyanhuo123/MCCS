
using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.DllNative.Models;
using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.StationSites;
using MCCS.Core.Models.Devices;
using MCCS.Core.Repositories;
using MCCS.Collecter.HardwareDevices;
using MCCS.Collecter.SignalManagers.Signals;
using MCCS.Common.DataManagers.Devices;
using MCCS.Common.DataManagers.Model3Ds;
using MCCS.Core.Models.StationSites;
using Microsoft.Extensions.Configuration;
using StationSiteInfo = MCCS.Common.DataManagers.StationSites.StationSiteInfo;

namespace MCCS.Services.StartInitial
{
    public class SplashService : ISplashService
    {
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IControllerManager _controllerService;
        private readonly IConfiguration _configuration;

        public SplashService(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository,
            IControllerManager controllerService,
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
            if (currentUseStation.Model3DAggregate == null) throw new ArgumentNullException("Station Site's Model3D is Null");
            var model3DAndControlChannels = await _stationSiteAggregateRepository.GetControlChannelAndModelInfoByModelIdAsync(currentUseStation.Model3DAggregate.BaseInfo.Id);
            var stationSiteInfo = new StationSiteInfo(currentUseStation.StationSiteInfo.Id, currentUseStation.StationSiteInfo.StationName);
            var allSignals = await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => c.IsDeleted == false);
            // 注册所有的设备
            var globalDevices = new List<BaseDevice>();
            foreach (var deviceInfo in deviceInfos)
            {
                if (deviceInfo.DeviceType == DeviceTypeEnum.Controller)
                { 
                    globalDevices.Add(new ControllerDevice(deviceInfo.Id, deviceInfo.DeviceName, null)); 
                }
                else if (deviceInfo.DeviceType == DeviceTypeEnum.Actuator)
                {
                    globalDevices.Add(new ActuatorDevice(deviceInfo.Id, deviceInfo.DeviceName, null));
                }
                else
                {
                    globalDevices.Add(new BaseDevice(deviceInfo.Id, deviceInfo.DeviceName, deviceInfo.DeviceType, null));
                }
            }
            // 所有的控制器接口Link设备;并绑定设备的父子关系;方便后面找到控制器进行控制
            foreach (var device in globalDevices.Where(c => c.Type == DeviceTypeEnum.Controller))
            {
                if (device is not ControllerDevice controllerDevice) continue;
                var bindSignals =
                    currentUseStation.Signals.Select(a =>
                    {
                        var temp1 = new StationSiteControllerSignalInfo(a.Id, a.BelongToControllerId, a.SignalName);
                        var bindDevice = globalDevices.FirstOrDefault(c => c.Id == a.ConnectedDeviceId); 
                        if (bindDevice != null)
                        {
                            bindDevice.ParentDeviceId = a.BelongToControllerId;
                            temp1.Link(bindDevice);
                        }
                        return temp1;
                    });
                controllerDevice.SignalInfos.AddRange(bindSignals);
            }
            GlobalDataManager.Instance.SetValue(globalDevices);
            // 创建所有的控制通道
            foreach (var controlChannelSignalInfo in currentUseStation.ControlChannelSignalInfos)
            {
                var tempChannel = new StationSiteControlChannelInfo(controlChannelSignalInfo.ControlChannelInfo.Id, controlChannelSignalInfo.ControlChannelInfo.ChannelName, controlChannelSignalInfo.ControlChannelInfo.ChannelType);
                foreach (var signal in controlChannelSignalInfo.Signals)
                {
                    if (GlobalDataManager.Instance.ControllerInfos == null) continue;
                    foreach (var signalInfoTemp in GlobalDataManager.Instance.ControllerInfos
                                 .Select(controller => controller.SignalInfos
                                 .FirstOrDefault(c => c.Id == signal.SignalInfo.Id))
                                 .OfType<StationSiteControllerSignalInfo>())
                    {
                        signalInfoTemp.ControlChannelSignalType = signal.SignalType;
                        tempChannel.BindSignals.Add(signalInfoTemp);
                        break;
                    }
                } 
                stationSiteInfo.ControlChannels.Add(tempChannel);
            }
            // 当前使用的所有3D模型
            var model3Ds = new List<Model3DMainInfo>();
            if (currentUseStation.Model3DAggregate != null && model3DAndControlChannels != null)
            {
                foreach (var modelFileItem in currentUseStation.Model3DAggregate.Model3DDataList)
                {
                    var temp = new Model3DMainInfo(modelFileItem.Id, modelFileItem.Key, modelFileItem.Name); 
                    var controlChannelIds = model3DAndControlChannels.Where(c => c.ModelFileId == modelFileItem.Key).Select(s => s.ControlChannelId).ToList();
                    var stationSiteControlChannels = stationSiteInfo.ControlChannels.Where(s => controlChannelIds.Contains(s.Id)).ToList();
                    temp.ControlChannelInfos = stationSiteControlChannels;
                    // 默认取一个控制通道的输出信号接口所链接的设备
                    var tempDevice = stationSiteControlChannels.FirstOrDefault()?.BindSignals
                        .FirstOrDefault(s => s.ControlChannelSignalType == SignalTypeEnum.Output)?.LinkedDevice;
                    temp.MappingDevice = tempDevice;
                    model3Ds.Add(temp);
                } 
            }
            GlobalDataManager.Instance.SetValue(model3Ds);
            StartUpAllControllers(deviceInfos
                .Where(c => c.DeviceType == DeviceTypeEnum.Controller), allSignals);
            GlobalDataManager.Instance.SetValue(stationSiteInfo); 
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
                    .Select(s =>
                    {
                        // var signalBinded = GlobalDataManager.Instance.ControllerInfos.Fi
                        return new HardwareSignalConfiguration
                        {
                            SignalId = s.Id,
                            SignalName = s.SignalName,
                            SignalAddress = (SignalAddressEnum)s.SignalAddress, 
                            MinValue = s.DownLimitRange,
                            MaxValue = s.UpLimitRange,
                            DeviceId = s.ConnectedDeviceId
                        };
                    }));
                _controllerService.CreateController(configuration);
            }
            _controllerService.StartAllControllers();
        }
    }
}
