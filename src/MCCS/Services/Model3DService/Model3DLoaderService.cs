using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Core.Models.Model3D;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoaderService : IModel3DLoaderService
    {
        private readonly SemaphoreSlim _importSemaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly SynchronizationContext? _uiContext;
        private readonly HelixToolkit.SharpDX.Core.EffectsManager _effectsManager;
        private readonly object _lockObject = new();
        private const int MaxConcurrentImports = 4;

        public event EventHandler<ModelImportedEventArgs> ModelImported;
        public event EventHandler<ImportProgressEventArgs> ImportProgress;
        public event EventHandler<ImportCompletedEventArgs> ImportCompleted;

        public Model3DLoaderService(HelixToolkit.SharpDX.Core.EffectsManager effectsManager)
        {
            _importSemaphore = new SemaphoreSlim(MaxConcurrentImports, MaxConcurrentImports);
            _cancellationTokenSource = new CancellationTokenSource();
            _uiContext = SynchronizationContext.Current;
            _effectsManager = effectsManager ?? throw new ArgumentNullException(nameof(effectsManager));
        }

        public bool IsImporting { get; private set; }

        /// <summary>
        /// 批量异步导入模型文件
        /// </summary>
        public async Task ImportModelsAsync(IEnumerable<string> filePaths,
            IProgress<ImportProgressEventArgs> progress = null,
            bool enableEnvironmentMap = true)
        {
            var filePathList = filePaths.ToList();
            var totalCount = filePathList.Count;
            var completedCount = 0;
            var importTasks = new List<Task>();

            foreach (var (filePath, index) in filePathList.Select((path, idx) => (path, idx)))
            {
                var task = ImportSingleModelAsync(filePath, index, enableEnvironmentMap, () =>
                {
                    var completed = Interlocked.Increment(ref completedCount);
                    progress?.Report(new ImportProgressEventArgs
                    {
                        CompletedCount = completed,
                        TotalCount = totalCount,
                        CurrentFile = filePath,
                        ProgressPercentage = (double)completed / totalCount * 100
                    });
                });

                importTasks.Add(task);
            }

            await Task.WhenAll(importTasks);

            _uiContext?.Post(_ =>
            {
                ImportCompleted?.Invoke(this, new ImportCompletedEventArgs
                {
                    TotalImported = completedCount,
                    CompletedAt = DateTime.Now
                });
            }, null);
        }

        public Task<IEnumerable<Model3DViewModel>> LoadModelsAsync(IEnumerable<Model3DData> modelDataList)
        {
            throw new NotImplementedException();
        }

        private async Task ImportSingleModelAsync(string filePath, int index,
            bool enableEnvironmentMap, Action? onCompleted)
        {
            await _importSemaphore.WaitAsync(_cancellationTokenSource.Token);

            try
            {
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var loader = new Importer();
                        var scene = loader.Load(filePath);

                        if (scene?.Root == null)
                            return new ImportResult
                            {
                                Success = false,
                                Error = new InvalidOperationException($"无法加载模型文件: {filePath}"),
                                FilePath = filePath,
                                Index = index
                            };
                        // 预附加场景图 - 重要优化
                        scene.Root.Attach(_effectsManager);
                        scene.Root.UpdateAllTransformMatrix();

                        // 配置材质
                        ConfigureMaterials(scene.Root, enableEnvironmentMap);

                        // 获取边界框和质心
                        scene.Root.TryGetBound(out var bound);
                        scene.Root.TryGetCentroid(out var centroid);

                        return new ImportResult
                        {
                            Success = true,
                            Scene = scene,
                            FilePath = filePath,
                            Index = index,
                            Bound = bound,
                            Centroid = centroid
                        };

                    }
                    catch (Exception ex)
                    {
                        return new ImportResult
                        {
                            Success = false,
                            Error = ex,
                            FilePath = filePath,
                            Index = index
                        };
                    }
                }, _cancellationTokenSource.Token);

                // 在UI线程中触发事件
                _uiContext?.Post(_ =>
                {
                    ModelImported?.Invoke(this, new ModelImportedEventArgs
                    {
                        ImportResult = result,
                        Index = index
                    });
                }, null);
            }
            finally
            {
                onCompleted?.Invoke();
                _importSemaphore.Release();
            }
        }

        private void ConfigureMaterials(SceneNode root, bool enableEnvironmentMap)
        {
            foreach (var node in root.Traverse())
            {
                if (node is not MaterialGeometryNode materialNode) continue;
                switch (materialNode.Material)
                {
                    case PBRMaterialCore pbrMaterial:
                        pbrMaterial.RenderEnvironmentMap = enableEnvironmentMap;
                        break;
                    case PhongMaterialCore phongMaterial:
                        phongMaterial.RenderEnvironmentMap = enableEnvironmentMap;
                        break;
                }
            }
        }

        public void CancelImport()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _importSemaphore.Dispose();
        }
    }
}
