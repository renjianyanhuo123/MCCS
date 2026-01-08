using MCCS.Infrastructure.Helper;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.DllNative.Models;
using MCCS.Station.Core.HardwareDevices;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;
using MCCS.Station.Core.SignalManagers.Signals;

using StationSiteInfo = MCCS.Station.Abstractions.Models.StationSiteInfo;

namespace MCCS.Station.Core
{
    public class StationRuntime(
        IControllerManager controllerManager,
        ISignalManager signalManager,
        IControlChannelManager controlChannelManager,
        IPseudoChannelManager pseudoChannelManager)
        : IStationRuntime
    {
        private const string _profileName = "stationProfile";
        private const string _stationMain = "station.main.json";

        private static readonly string _currentStationDirectoryPath = Path.Combine(AppContext.BaseDirectory, _profileName);

        public bool IsExistCurrentStationProfile()
        {
            FileHelper.EnsureDirectoryExists(_currentStationDirectoryPath);
            return FileHelper.FileExists(Path.Combine(_currentStationDirectoryPath, _stationMain));
        }

        public void BuildCurrentStationProfile(StationSiteInfo stationSiteInfo) => FileHelper.WriteJson(Path.Combine(_currentStationDirectoryPath, _stationMain), stationSiteInfo);

        public async Task BuildCurrentStationProfileAsync(StationSiteInfo stationSiteInfo,
            CancellationToken cancellationToken = default) =>
            await FileHelper.WriteJsonAsync(Path.Combine(_currentStationDirectoryPath, _stationMain), stationSiteInfo);

        public async Task<StationSiteInfo?> GetStationInfoBtProfileAsync(CancellationToken cancellationToken = default) => 
            await FileHelper.ReadJsonAsync<StationSiteInfo>(Path.Combine(_currentStationDirectoryPath, _stationMain));

        public async Task<StationSiteInfo> InitialStationSiteAsync(bool isMock, CancellationToken cancellationToken = default)
        {
            var stationSiteInfo = await GetStationInfoBtProfileAsync(cancellationToken);
            if (stationSiteInfo == null) throw new ArgumentNullException(nameof(stationSiteInfo)); 
            if (!controllerManager.InitializeDll(isMock))throw new InvalidOperationException("Controller Initialize Dll Error");
            var index = 0;
            // (1)初始化注册所有的控制器
            foreach (var item in stationSiteInfo.Controllers)
            {
                var configuration = new HardwareDeviceConfiguration
                {
                    DeviceId = item.Id,
                    DeviceAddressId = index++,
                    DeviceName = item.Name,
                    DeviceType = item.Type.ToString(),
                    IsSimulation = isMock,
                    SampleRate = 100,
                    ConnectionString = ""
                };
                controllerManager.CreateController(configuration);
            }
            // (2) 初始化注册所有的信号接口
            signalManager.Initialization(stationSiteInfo.AllSignals.Select(s => new HardwareSignalConfiguration
            {
                SignalId = s.Id,
                SignalName = s.Name,
                SignalAddress = (SignalAddressEnum)s.SignalAddress,
                MinValue = s.DownLimit,
                MaxValue = s.UpLimit,
                Unit = s.Unit,
                BelongControllerId = s.BelongControllerId,
                DeviceId = s.LinkedDeviceId
            }));
            
            // (3) 初始化所有的控制通道
            var controlChannelConfigurations = new List<ControlChannelConfiguration>();
            foreach (var controlChannelSignalInfo in stationSiteInfo.ControlChannels)
            {
                var tempChannel = new ControlChannelConfiguration
                {
                    ChannelId = controlChannelSignalInfo.Id,
                    ChannelName = controlChannelSignalInfo.Name,
                    ControllerId = controlChannelSignalInfo.ControllerId,
                    SignalConfiguration = controlChannelSignalInfo.BindSignalIds.Select(s => new ControlChannelSignalConfiguration
                    {
                        SignalId = s.SignalId,
                        SignalType = (ControlChannelSignalTypeEnum)s.SignalType
                    }).ToList()
                }; 
                controlChannelConfigurations.Add(tempChannel);
            }
            controlChannelManager.Initialization(controlChannelConfigurations);
            // (4) 创建所有的虚拟通道
            var pseudoChannelConfigurations = new List<PseudoChannelConfiguration>(); 
            foreach (var pseudoChannelInfo in stationSiteInfo.PseudoChannels)
            {
                var tempPseudoChannel = new PseudoChannelConfiguration
                {
                    ChannelId = pseudoChannelInfo.Id,
                    ChannelName = pseudoChannelInfo.Name,
                    Formula = pseudoChannelInfo.Formula,
                    HasTare = pseudoChannelInfo.HasTare,
                    RangeMax = pseudoChannelInfo.RangeMax,
                    RangeMin = pseudoChannelInfo.RangeMin,
                    Unit = pseudoChannelInfo.Unit,
                    SignalIds = pseudoChannelInfo.BindSignalIds
                };
                pseudoChannelConfigurations.Add(tempPseudoChannel);
            }
            pseudoChannelManager.Initialization(pseudoChannelConfigurations); 
           
            return stationSiteInfo;
        }
    }
}
