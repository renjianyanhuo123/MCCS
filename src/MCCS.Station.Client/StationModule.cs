using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core;

namespace MCCS.Station.Client
{
    public sealed class StationModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        { 
            containerRegistry.Register<IStationRuntime>(c => new StationRuntime());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
    }
}
