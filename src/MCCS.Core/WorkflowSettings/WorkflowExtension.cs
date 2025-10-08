using MCCS.WorkflowSetting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MCCS.Core.WorkflowSettings
{
    public static class WorkflowExtension
    {
        public static IServiceCollection AddWorkflowSteps(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IWorkflowCanvasRenderer, WorkflowCanvasRenderer>();
            services.AddWorkflow();
            return services;
        }
    }
}
