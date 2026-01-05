using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.StationSites;
using MCCS.Common.DataManagers.Devices;
using MCCS.Common.DataManagers.Model3Ds;
using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Repositories; 
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;
using MCCS.Station.Core.SignalManagers.Signals; 

using Microsoft.Extensions.Configuration;
using StationSiteInfo = MCCS.Common.DataManagers.StationSites.StationSiteInfo;

namespace MCCS.Services.StartInitial
{
    public class SplashService : ISplashService
    {
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository; 
        private readonly IConfiguration _configuration;   
        private readonly bool _isMock;

        public SplashService(IStationSiteAggregateRepository stationSiteAggregateRepository, 
            IDeviceInfoRepository deviceInfoRepository,  
            IConfiguration configuration)
        {
            _isMock = Convert.ToBoolean(configuration["AppSettings:IsMock"]); 
            _configuration = configuration;
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;   
        }

        public async Task InitialHardwareDevicesAsync()
        {
            var currentUseStation = await _stationSiteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (currentUseStation?.StationSiteInfo == null) throw new ArgumentNullException("Current Use Station Site is Null");
            var deviceInfos = await _deviceInfoRepository.GetAllDevicesAsync();
            if (deviceInfos == null) throw new ArgumentNullException("Current Controllers is Null");
            if (currentUseStation.Model3DAggregate == null || currentUseStation.Model3DAggregate.BaseInfo == null)
                throw new ArgumentNullException("Station Site's Model3D or Model3D BaseInfo is Null");
            //var model3DAndControlChannels = await _stationSiteAggregateRepository.GetControlChannelAndModelInfoByModelIdAsync(currentUseStation.Model3DAggregate.BaseInfo.Id);
            //var stationSiteInfo = new StationSiteInfo(currentUseStation.StationSiteInfo.Id, currentUseStation.StationSiteInfo.StationName);
            //var allSignals = await _deviceInfoRepository.GetSignalInterfacesByExpressionAsync(c => c.IsDeleted == false);
            // StartUpAllControllers(deviceInfos.Where(c => c.DeviceType == DeviceTypeEnum.Controller), allSignals);
            //// 注册所有的设备
            //var globalDevices = new List<BaseDevice>();
            //foreach (var deviceInfo in deviceInfos)
            //{
            //    if (deviceInfo.DeviceType == DeviceTypeEnum.Controller)
            //    { 
            //        globalDevices.Add(new ControllerDevice(deviceInfo.Id, deviceInfo.DeviceName, null)); 
            //    }
            //    else if (deviceInfo.DeviceType == DeviceTypeEnum.Actuator)
            //    {
            //        globalDevices.Add(new ActuatorDevice(deviceInfo.Id, deviceInfo.DeviceName, null));
            //    }
            //    else
            //    {
            //        globalDevices.Add(new BaseDevice(deviceInfo.Id, deviceInfo.DeviceName, deviceInfo.DeviceType, null));
            //    }
            //}
            //// 所有的控制器接口Link设备;并绑定设备的父子关系;方便后面找到控制器进行控制
            //foreach (var device in globalDevices.Where(c => c.Type == DeviceTypeEnum.Controller))
            //{
            //    if (device is not ControllerDevice controllerDevice) continue;
            //    var bindSignals =
            //        currentUseStation.Signals.Select(a =>
            //        {
            //            var temp1 = new StationSiteControllerSignalInfo(a.Id, a.BelongToControllerId, a.SignalName);
            //            var bindDevice = globalDevices.FirstOrDefault(c => c.Id == a.ConnectedDeviceId); 
            //            if (bindDevice != null)
            //            {
            //                bindDevice.ParentDeviceId = a.BelongToControllerId;
            //                temp1.Link(bindDevice);
            //            }
            //            return temp1;
            //        });
            //    controllerDevice.SignalInfos.AddRange(bindSignals);
            //}
            //GlobalDataManager.Instance.SetValue(globalDevices);
            //// 创建所有的控制通道 
            //var controlChannelConfigurations = new List<ControlChannelConfiguration>(); 
            //foreach (var controlChannelSignalInfo in currentUseStation.ControlChannelSignalInfos)
            //{
            //    var tempChannel = new ControlChannelConfiguration
            //    {
            //        ChannelId = controlChannelSignalInfo.ControlChannelInfo.Id,
            //        ChannelName = controlChannelSignalInfo.ControlChannelInfo.ChannelName,
            //        ControllerId = controlChannelSignalInfo.ControlChannelInfo.ControllerId,
            //        CancellationConfiguration = new ControlCompletionConfiguration(),
            //        SignalConfiguration = []
            //    }; 
            //    foreach (var signal in controlChannelSignalInfo.Signals)
            //    {
            //        tempChannel.SignalConfiguration.Add(new ControlChannelSignalConfiguration
            //        {
            //            SignalId = signal.SignalInfo.Id,
            //            BelongControllerId = signal.SignalInfo.BelongToControllerId,
            //            SignalName = signal.SignalInfo.SignalName,
            //            SignalAddress = (SignalAddressEnum)signal.SignalInfo.SignalAddress,
            //            SignalType = (ControlChannelSignalTypeEnum)signal.SignalType,
            //            DeviceId = signal.SignalInfo.ConnectedDeviceId
            //        });
            //    }
            //    controlChannelConfigurations.Add(tempChannel);
            //}
            //_controlChannelManager.Initialization(controlChannelConfigurations, _isMock);
            //var pseudoChannelConfigurations = new List<PseudoChannelConfiguration>();
            //// 创建所有的虚拟通道
            //foreach (var pseudoChannelInfo in currentUseStation.PseudoChannelInfos)
            //{
            //    var tempPseudoChannel = new PseudoChannelConfiguration
            //    {
            //        ChannelId = pseudoChannelInfo.PseudoChannelInfo.Id,
            //        ChannelName = pseudoChannelInfo.PseudoChannelInfo.ChannelName,
            //        Formula = pseudoChannelInfo.PseudoChannelInfo.Formula,
            //        HasTare = pseudoChannelInfo.PseudoChannelInfo.HasTare,
            //        RangeMax = pseudoChannelInfo.PseudoChannelInfo.RangeMax,
            //        RangeMin = pseudoChannelInfo.PseudoChannelInfo.RangeMin,
            //        Unit = pseudoChannelInfo.PseudoChannelInfo.Unit,
            //        SignalConfigurations = [.. pseudoChannelInfo.Signals.Select(s => new HardwareSignalConfiguration
            //        {
            //            SignalId = s.Id,
            //            SignalName = s.SignalName,
            //            BelongControllerId = s.BelongToControllerId,
            //            SignalAddress = (SignalAddressEnum)s.SignalAddress
            //        })]
            //    };
            //    pseudoChannelConfigurations.Add(tempPseudoChannel);
            //}
            //_pseudoChannelManager.Initialization(pseudoChannelConfigurations);
            //// 当前使用的所有3D模型
            //var model3Ds = new List<Model3DMainInfo>();
            //if (currentUseStation.Model3DAggregate != null && model3DAndControlChannels != null)
            //{
            //    foreach (var modelFileItem in currentUseStation.Model3DAggregate.Model3DDataList)
            //    {
            //        var temp = new Model3DMainInfo(modelFileItem.Id, modelFileItem.Key, modelFileItem.Name)
            //            {
            //                ControlChannelInfos = currentUseStation.ControlChannelSignalInfos.Where(s => model3DAndControlChannels.Where(c => c.ModelFileId == modelFileItem.Key).Select(c => c.ControlChannelId).Contains(s.ControlChannelInfo.Id))
            //                    .Select(s => new StationSiteControlChannelInfo(s.ControlChannelInfo.Id, s.ControlChannelInfo.ChannelName, s.ControlChannelInfo.ChannelType))
            //                    .ToList()
            //            };
            //        // 默认取一个控制通道的输出信号接口所链接的设备
            //        // var tempDevice = deviceInfos.FirstOrDefault(c => c.Id == modelFileItem.MapDeviceId); 
            //        model3Ds.Add(temp);
            //    } 
            //}
            //GlobalDataManager.Instance.SetValue(model3Ds); 
            //GlobalDataManager.Instance.SetValue(stationSiteInfo); 
        }


        /// <summary>
        /// 注册并启动所有控制器
        /// </summary>
        /// <param name="controllerInfos">所有的控制器设备</param>
        /// <param name="signals">所有的物理信号接口</param>
        /// <returns></returns>
        //private void StartUpAllControllers(IEnumerable<DeviceInfo> controllerInfos,
        //    List<SignalInterfaceInfo> signals)
        //{ 
        //    _controllerService.InitializeDll(_isMock);
        //    var index = 0;
        //    foreach (var item in controllerInfos)
        //    {
        //        var configuration = new HardwareDeviceConfiguration
        //        {
        //            DeviceId = item.Id,
        //            DeviceAddressId = index++,
        //            DeviceName = item.DeviceName,
        //            DeviceType = item.DeviceType.ToString(),
        //            IsSimulation = _isMock, 
        //            SampleRate = 100,
        //            ConnectionString = ""
        //        }; 
        //        _controllerService.CreateController(configuration);
        //    }
        //    // 初始化注册所有的信号接口
        //    _signalManager.Initialization(signals.Select(s => new HardwareSignalConfiguration
        //    {
        //        SignalId = s.Id,
        //        SignalName = s.SignalName,
        //        SignalAddress = (SignalAddressEnum)s.SignalAddress,
        //        MinValue = s.DownLimitRange,
        //        MaxValue = s.UpLimitRange,
        //        Unit = s.Unit,
        //        BelongControllerId = s.BelongToControllerId,
        //        DeviceId = s.ConnectedDeviceId
        //    }));
        //    _controllerService.StartAllControllers();
        //}
    }
}
