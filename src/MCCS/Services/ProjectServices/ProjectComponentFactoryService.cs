using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Interface.Components.Models;
using MCCS.Interface.Components.ViewModels;

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
            if (cellNode.ParamterJson == null) return null;
            switch (componentModel.ViewTypeName)
            {
                case nameof(ProjectChartComponentPageViewModel): 
                    var parameter1 = JsonConvert.DeserializeObject<ChartSettingParamModel>(cellNode.ParamterJson);
                    if (parameter1 == null) return null;
                    return new ProjectChartComponentPageViewModel(parameter1);
                case nameof(ProjectDataMonitorComponentPageViewModel):
                    var parameter2 = JsonConvert.DeserializeObject<List<DataMonitorSettingItemParamModel>>(cellNode.ParamterJson);
                    if (parameter2 == null) return null;
                    return new ProjectDataMonitorComponentPageViewModel(parameter2); 
                default:
                    return new ProjectDataMonitorComponentPageViewModel(null);
            } 
        }
    }
}
