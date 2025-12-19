using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp; 
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;
using Microsoft.Extensions.Configuration;
using MCCS.Common;
using MCCS.Infrastructure.Models.Model3D;
using SharpDX;
using Color = System.Windows.Media.Color;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoaderService(
        IEffectsManager effectsManager, 
        IConfiguration configuration)
        : IModel3DLoaderService
    {
        private readonly SemaphoreSlim _importSemaphore = new(_maxConcurrentImports, _maxConcurrentImports); 
        private readonly  IEffectsManager _effectsManager = effectsManager ?? throw new ArgumentNullException(nameof(effectsManager)); 
        private const int _maxConcurrentImports = 4; 
        private readonly IConfiguration _configuration = configuration;
        private readonly object _lock = new();

        public async Task<IList<Model3DViewModel>> ImportModelsAsync(
            List<Model3DData> modelInfos,
            IProgress<ImportProgressEventArgs> progress,
            Color materialColor,
            CancellationToken cancellationToken)
        {
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
                    var result = await ImportSingleModelAsync(modelInfo, materialColor, cancellationToken);
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
            Color material,
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
                return new Model3DViewModel(scene.Root, modelInfo, material);
            }, cancellationToken);

            return result;
        }
    }
}
