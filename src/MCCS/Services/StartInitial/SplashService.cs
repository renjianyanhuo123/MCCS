using MCCS.Common.DataManagers;
using MCCS.Infrastructure.Repositories;
using MCCS.Infrastructure.Services;
using MCCS.Station.Abstractions.Interfaces;

using Microsoft.Extensions.Configuration;

namespace MCCS.Services.StartInitial
{
    public class SplashService : ISplashService
    {
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly IStationRuntime _stationRuntime;
        private readonly IConfiguration _configuration;   
        private readonly bool _isMock;
        private readonly MCCS.Services.StationServices.IStationService _stationService;

        public SplashService(IStationSiteAggregateRepository stationSiteAggregateRepository, 
            IDeviceInfoRepository deviceInfoRepository,
            IStationRuntime stationRuntime,
            MCCS.Services.StationServices.IStationService stationService,
            IConfiguration configuration)
        {
            _stationRuntime = stationRuntime;
            _stationService = stationService;
            _isMock = Convert.ToBoolean(configuration["AppSettings:IsMock"]); 
            _configuration = configuration;
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;   
        }

        public async Task InitialHardwareDevicesAsync()
        {
            if (!_stationRuntime.IsExistCurrentStationProfile())
            {  
                var stationInfo = await _stationService.GetCurrentStationSiteInfoAsync();
                if (stationInfo == null) throw new ArgumentNullException("Current Use Station Site is Null");
                await _stationRuntime.BuildCurrentStationProfileAsync(stationInfo);
            }
            if(GlobalDataManager.Instance.ProcessManager != null) 
                GlobalDataManager.Instance.ProcessManager.Start();
            // 初始化（通常在应用启动时调用一次）(共享内存初始化数据)
            await ChannelDataServiceProvider.EnsureStartedAsync();
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
