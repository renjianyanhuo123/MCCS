using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using MCCS.Core.Models.Model3D;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;
using MCCS.Core.Repositories;
using Microsoft.Extensions.Configuration;
using MCCS.Common;
using SharpDX;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoaderService : IModel3DLoaderService
    {
        private readonly SemaphoreSlim _importSemaphore;
        private CancellationTokenSource _cancellationTokenSource;
        // private readonly SynchronizationContext? _uiContext;
        private readonly  HelixToolkit.SharpDX.Core.EffectsManager _effectsManager; 
        private const int MaxConcurrentImports = 4;
        private readonly IModel3DDataRepository _modelRepository;
        private readonly IConfiguration _configuration;

        public Model3DLoaderService(
            HelixToolkit.SharpDX.Core.EffectsManager effectsManager,
            IModel3DDataRepository model3DDataRepository,
            IConfiguration configuration)
        {
            _importSemaphore = new SemaphoreSlim(MaxConcurrentImports, MaxConcurrentImports);
            // _uiContext = SynchronizationContext.Current;
            _effectsManager = effectsManager ?? throw new ArgumentNullException(nameof(effectsManager));
            _modelRepository = model3DDataRepository;
            _configuration = configuration;
        } 

        public async Task<IList<Model3DViewModel>> ImportModelsAsync(
            IProgress<ImportProgressEventArgs> progress,
            CancellationToken cancellationToken)
        {
            var groupKey = _configuration["AppSettings:ModelKey"]
                           ?? throw new ArgumentNullException("AppSettings:ModelKey");
            var modelInfos = await _modelRepository.GetModelAsync(groupKey, cancellationToken);
            var viewModels = new List<Model3DViewModel>();

            var progressInfo = new ImportProgressEventArgs { TotalCount = modelInfos.Count };

            var tasks = modelInfos.Select(async modelInfo =>
            {
                await _importSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ImportSingleModelAsync(modelInfo, cancellationToken);

                    lock (viewModels)
                    {
                        viewModels.Add(result);
                        progressInfo.CompletedCount++;
                        progressInfo.CurrentFileName = modelInfo.Name;
                        progress?.Report(progressInfo);
                    }
                    return result;
                }
                finally
                {
                    _importSemaphore.Release();
                }
            });
            var importResults = await Task.WhenAll(tasks);
            return importResults.ToList();
        }

        public async Task<Model3DViewModel> ImportSingleModelAsync(
            Model3DData modelInfo,
            CancellationToken cancellationToken = default)
        {
            var result = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var loader = new Importer();
                var scene = loader.Load(modelInfo.FilePath);

                if (scene?.Root == null) throw new InvalidOperationException($"无法加载模型文件: {modelInfo.FilePath}"); 
                // 应用变换
                var transform = Matrix.Scaling(modelInfo.ScaleStr.ToVector<Vector3>())
                                * Matrix.RotationAxis(modelInfo.RotationStr.ToVector<Vector3>(), 10) 
                                * Matrix.Translation(modelInfo.PositionStr.ToVector<Vector3>());
                scene.Root.ModelMatrix = transform;
                // 预附加场景图以优化性能
                scene.Root.Attach(_effectsManager);
                scene.Root.UpdateAllTransformMatrix();
                return new Model3DViewModel(scene.Root, modelInfo);
            }, cancellationToken);

            return result;
        }
    }
}
