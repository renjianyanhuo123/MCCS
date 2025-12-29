using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;

namespace MCCS.Services.ProjectServices
{
    public interface IProjectComponentFactoryService
    {
        object? BuildComponentViewModel(CellNode cellNode);
    }
}
