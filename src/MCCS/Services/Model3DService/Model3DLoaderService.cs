using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using MCCS.Core.Models.Model3D;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;
using Microsoft.Extensions.Configuration;
using MCCS.Common;
using SharpDX;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoaderService(
        IEffectsManager effectsManager, 
        IConfiguration configuration)
        : IModel3DLoaderService
    {
        private readonly SemaphoreSlim _importSemaphore = new(MaxConcurrentImports, MaxConcurrentImports);
        // private CancellationTokenSource _cancellationTokenSource;
        // private readonly SynchronizationContext? _uiContext;
        private readonly  IEffectsManager _effectsManager = effectsManager ?? throw new ArgumentNullException(nameof(effectsManager)); 
        private const int MaxConcurrentImports = 4; 
        private readonly IConfiguration _configuration = configuration;
        private readonly object _lock = new();

        public async Task<IList<Model3DViewModel>> ImportModelsAsync(
            List<Model3DData> modelInfos,
            IProgress<ImportProgressEventArgs> progress,
            CancellationToken cancellationToken)
        {
            //if (GlobalDataManager.Instance.StationSiteInfo == null) throw new ArgumentNullException("StationSiteInfo is NULL");
            //var modelAggregate = await _modelRepository.GetModelAggregateByStationIdAsync(GlobalDataManager.Instance.StationSiteInfo.Id, cancellationToken); 
            var viewModels = new List<Model3DViewModel>(); 
            var progressInfo = new ImportProgressEventArgs
            {
                CompletedCount = 0,
                ProgressPercentage = 0,
                TotalCount = modelInfos.Count
            };
            progress.Report(progressInfo);
            var tasks = modelInfos.Select(async modelInfo =>
            {
                await _importSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ImportSingleModelAsync(modelInfo, cancellationToken);
                    //await Task.Delay(3000, cancellationToken); // 测试使用
                    lock (_lock)
                    {
                        viewModels.Add(result);
                        progressInfo.CompletedCount++;
                        progressInfo.CurrentFileName = modelInfo.Name;
                        progressInfo.ProgressPercentage = (double)progressInfo.CompletedCount / progressInfo.TotalCount * 100;
                        progress.Report(new ImportProgressEventArgs
                        {
                            CompletedCount = progressInfo.CompletedCount,
                            CurrentFileName = progressInfo.CurrentFileName,
                            ProgressPercentage = progressInfo.ProgressPercentage,
                            TotalCount = progressInfo.TotalCount
                        });
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
                const double angle = 0;
                // 应用变换
                var transform = Matrix.Scaling(modelInfo.ScaleStr.ToVector<Vector3>())
                                * Matrix.RotationAxis(modelInfo.RotationStr.ToVector<Vector3>(), (float)angle.ToRadian()) 
                                * Matrix.Translation(modelInfo.PositionStr.ToVector<Vector3>());
                scene.Root.ModelMatrix = transform;
                // 预附加场景图以优化性能
                scene.Root.Attach(_effectsManager);
                scene.Root.UpdateAllTransformMatrix();
                loader.Dispose();
                return new Model3DViewModel(scene.Root, modelInfo);
            }, cancellationToken);

            return result;
        }
    }
}
