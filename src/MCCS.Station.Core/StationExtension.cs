using MCCS.Station.Abstractions.Interfaces;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.PseudoChannelManagers;
using MCCS.Station.Core.SignalManagers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Station.Core
{
    public static class StationExtension
    {
        public static IServiceCollection AddStationCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IControllerManager, ControllerManager>();
            services.AddSingleton<ISignalManager, SignalManager>();
            services.AddSingleton<IControlChannelManager, ControlChannelManager>();
            services.AddSingleton<IPseudoChannelManager, PseudoChannelManager>();
            services.AddTransient<IStationRuntime, StationRuntime>();
            return services;
        }
    }
}
