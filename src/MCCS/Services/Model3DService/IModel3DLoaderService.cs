using MCCS.Core.Models.Model3D;
using MCCS.ViewModels.Others;

namespace MCCS.Services.Model3DService
{
    public interface IModel3DLoaderService
    {
        public Task<Model3DViewModel> LoadModelAsync(Model3DData modelData);
    }
}
