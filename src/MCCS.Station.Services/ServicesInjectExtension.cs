using MCCS.Station.Services.IServices;
using MCCS.Station.Services.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Station.Services;

public static class ServicesInjectExtension
{
    public static IServiceCollection AddCommandServiceCollection(this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddSingleton<IOperationValveService, OperationValveService>();
        services.AddSingleton<IOperationTestService, OperationTestService>();
        return services;
    }
}