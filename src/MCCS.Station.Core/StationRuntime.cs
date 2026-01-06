using System.Diagnostics;
using System.Linq;

using MCCS.Infrastructure.Domain.StationSites;
using MCCS.Infrastructure.Helper;
using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Abstractions.Models;

using StationSiteInfo = MCCS.Station.Abstractions.Models.StationSiteInfo;

namespace MCCS.Station.Core
{
    public class StationRuntime : IStationRuntime
    {
        private const string _profileName = "stationProfile";
        private const string _stationMain = "station.main.json";
        //private const string _stationProcessName = "MCCS.Station.Host.exe";
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

        public StationSiteInfo MappingStationSiteInfo(StationSiteAggregate aggregateInfo)
        {
            var controlChannels = aggregateInfo.ControlChannelSignalInfos
                .Select(s => new StationSiteControlChannelInfo(s.ControlChannelInfo.Id, s.ControlChannelInfo.ChannelName, s.ControlChannelInfo.ChannelType)
            {
                BindSignals = s.Signals.Select(c => new StationSiteControllerSignalInfo(c.SignalInfo.Id, c.SignalInfo.BelongToControllerId, c.SignalInfo.SignalName)
                {
                    ControlChannelSignalType = c.SignalType,
                    LinkedDevice = c.LinkDeviceInfo == null ? null : new BaseDevice(c.LinkDeviceInfo.Id, c.LinkDeviceInfo.DeviceName, c.LinkDeviceInfo.DeviceType, null)
                }).ToList()
            }).ToList();
            var stationInfo = new StationSiteInfo(aggregateInfo.StationSiteInfo.Id,
                aggregateInfo.StationSiteInfo.StationName, controlChannels);
            return stationInfo;
        }
    }
}
