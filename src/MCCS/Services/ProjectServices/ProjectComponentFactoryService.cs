using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Models.MethodManager.ParamterSettings;
using MCCS.ViewModels.ProjectManager.Components;

using Newtonsoft.Json;

namespace MCCS.Services.ProjectServices
{
    /// <summary>
    /// 全局单例
    /// 可以做成缓存
    /// </summary>
    public sealed class ProjectComponentFactoryService : IProjectComponentFactoryService
    {
        private readonly IMethodRepository _methodRepository;
        private static List<MethodUiComponentsModel>? _componentModels;

        public ProjectComponentFactoryService(IMethodRepository methodRepository)
        {
            _methodRepository = methodRepository;
        }

        public object? BuildComponentViewModel(CellNode cellNode)
        {
            _componentModels ??= _methodRepository.GetUiComponents();
            var componentModel = _componentModels.FirstOrDefault(c => c.Id == cellNode.NodeId);
            if (componentModel == null) return null;
            switch (componentModel.ViewTypeName)
            {
                case nameof(ProjectChartComponentPageViewModel):
                    if (cellNode.ParamterJson == null) return null;
                    var parameter = JsonConvert.DeserializeObject<ChartSettingParamModel>(cellNode.ParamterJson);
                    if (parameter == null) return null;
                    return new ProjectChartComponentPageViewModel(parameter);
                case nameof(ProjectDataMonitorComponentPageViewModel):
                default:
                    return new ProjectDataMonitorComponentPageViewModel();
            } 
        }
    }
}
