using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Infrastructure.WorkflowSettings
{
    public static class WorkflowExtension
    {
        public static IServiceCollection AddWorkflowSteps(this IServiceCollection services, IConfiguration configuration)
        { 
            services.AddWorkflow();
            return services;
        }
    }
}
