using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

namespace MCCS.Station.Client
{
    public sealed class StationModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IControllerManager, ControllerManager>();
            containerRegistry.RegisterSingleton<ISignalManager, SignalManager>();
            containerRegistry.RegisterSingleton<IControlChannelManager, ControlChannelManager>();
            containerRegistry.RegisterSingleton<IPseudoChannelManager, PseudoChannelManager>();
            containerRegistry.RegisterSingleton<IStationRuntime, StationRuntime>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
    }
}
