using MCCS.Infrastructure.Models.Devices;
using MCCS.Infrastructure.Repositories;
using MCCS.Station.Abstractions.Models;

namespace MCCS.Services.StationServices
{
    internal class StationService : IStationService
    {
        private readonly IStationSiteAggregateRepository _stionStationSiteAggregateRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;

        public StationService(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IDeviceInfoRepository deviceInfoRepository)
        {
            _stionStationSiteAggregateRepository = stationSiteAggregateRepository;
            _deviceInfoRepository = deviceInfoRepository;
        }

        public async Task<StationSiteInfo> GetCurrentStationSiteInfoAsync()
        {
            var devices = await _deviceInfoRepository.GetAllDevicesAsync();
            var stationMainInfo = await _stionStationSiteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            var res = new StationSiteInfo(stationMainInfo.StationSiteInfo.Id,
                stationMainInfo.StationSiteInfo.StationName,
                stationMainInfo.ControlChannelSignalInfos
                    .Select(s => new StationSiteControlChannelInfo(s.ControlChannelInfo.Id,
                    s.ControlChannelInfo.ChannelName,
                    s.ControlChannelInfo.ChannelType,
                    s.ControlChannelInfo.ControllerId)
                    {
                        BindSignalIds = s.Signals.Select(c => new StationSiteControlChannelSignalInfo
                        {
                            SignalId = c.SignalId,
                            SignalType = c.SignalType
                        }).ToList()
                    }).ToList(),
                stationMainInfo.PseudoChannelInfos.Select(s => new StationSitePseudoChannelInfo(
                    s.PseudoChannelInfo.Id, 
                    s.PseudoChannelInfo.ChannelName,
                    s.PseudoChannelInfo.Formula,
                    s.PseudoChannelInfo.HasTare,
                    s.PseudoChannelInfo.RangeMax,
                    s.PseudoChannelInfo.RangeMin,
                    s.PseudoChannelInfo.Unit ?? "")
                {
                    BindSignalIds = s.SignalIds
                }).ToList(),
                stationMainInfo.Signals.Select(s => new StationSiteControllerSignalInfo(s.Id, s.BelongToControllerId, s.SignalName, s.SignalAddress, s.UpLimitRange, s.DownLimitRange, s.Unit)).ToList(),
                devices.Where(c => c is { IsDeleted: false, DeviceType: DeviceTypeEnum.Controller }).Select(s => new ControllerDevice(s.Id, s.DeviceName)).ToList(),
                devices.Where(s => s.DeviceType != DeviceTypeEnum.Controller).Select(s => new BaseDevice(s.Id, s.DeviceName, s.DeviceType)).ToList()
                );
            return res;
        }
    }
}
