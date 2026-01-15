using System.Collections.ObjectModel;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.ViewModels.Parameters;

using Serilog;

namespace MCCS.Interface.Components.ViewModels
{
    /// <summary>
    /// 数据监控组件 ViewModel
    /// 演示：构造函数同时支持 DI 服务注入 + 业务参数
    /// </summary>
    [InterfaceComponent(
        "data-monitor-component",
        "数据监控组件",
        InterfaceComponentCategory.Display,
        Description = "用于实时监控和显示测试数据",
        Icon = "MonitorDashboard",
        IsCanSetParam = true,
        SetParamViewName = nameof(DataMonitorSetParamPageViewModel),
        Order = 2)]
    public sealed class ProjectDataMonitorComponentPageViewModel : BaseComponentViewModel
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 构造函数 - 支持 DI 注入和业务参数
        /// </summary>
        /// <param name="logger">从 DI 容器自动注入的日志服务</param>
        /// <param name="parameters">业务参数（从外部传入）</param>
        public ProjectDataMonitorComponentPageViewModel(
            ILogger logger,
            List<DataMonitorSettingItemParamModel> parameters)
        {
            _logger = logger;
            _logger.Debug("创建数据监控组件，参数数量: {Count}", parameters?.Count ?? 0);

            Chilldren.Clear();
            if (parameters == null) return;

            foreach (var paramter in parameters)
            {
                Chilldren.Add(new ProjectDataMonitorComponentItemModel
                {
                    Id = paramter.PseudoChannel.Id,
                    DisplayName = paramter.PseudoChannel.DisplayName,
                    Unit = paramter.PseudoChannel.Unit,
                    RetainBit = paramter.RetainBit,
                    Value = 0.0
                });
            }
        }

        #region Property
        public ObservableCollection<ProjectDataMonitorComponentItemModel> Chilldren { get; } = [];
        #endregion
    }
}
