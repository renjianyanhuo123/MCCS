using System.Windows.Media;
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
            Color materialColor,
            CancellationToken cancellationToken = default);

        public Task<Model3DViewModel> ImportSingleModelAsync(
            Model3DData modelInfo,
            Color materialColor,
            CancellationToken cancellationToken = default);
    }
}
