using MCCS.Core.Models.Model3D;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;

namespace MCCS.Services.Model3DService
{
    public interface IModel3DLoaderService
    {
        public Task<IList<Model3DViewModel>> ImportModelsAsync(
            List<Model3DData> modelInfos,
            IProgress<ImportProgressEventArgs> progress,
            CancellationToken cancellationToken = default);

        public Task<Model3DViewModel> ImportSingleModelAsync(
            Model3DData modelInfo,
            CancellationToken cancellationToken = default);
    }
}
