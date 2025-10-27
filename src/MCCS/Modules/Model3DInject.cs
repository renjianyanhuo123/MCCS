using HelixToolkit.SharpDX.Core; 
using MCCS.Services.Model3DService;
using Microsoft.Extensions.Configuration;

namespace MCCS.Modules
{
    public static class Model3DInject
    {
        public static void AddModel3DServices(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            // Register services related to Model3D
            // For example:
            // ServiceCollection.AddModel3DServices<IModel3DLoaderService, Model3DLoaderService>();
            // ServiceCollection.AddSingleton<IModel3DDataRepository, Model3DDataRepository>();
            // Note: The actual implementation of these services should be defined in their respective files.
            // var effectManager = new DefaultEffectsManager();
            containerRegistry.RegisterSingleton<IEffectsManager, DefaultEffectsManager>();
            containerRegistry.Register<IModel3DLoaderService, Model3DLoaderService>();
            // containerRegistry.RegisterSingleton<IGenerateControlCommandServices, GenerateControlCommandServices>();

        }
    }
}
