using System.Collections.ObjectModel;

using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.ViewModels.Parameters;

namespace MCCS.Interface.Components.ViewModels
{
    public sealed class ProjectDataMonitorComponentPageViewModel : BindableBase
    {
        /// <summary>
        /// 当外部传入List<DataMonitorSettingItemParamModel>  会发生装箱，因为List.GetEnumetor(值类型),然后转换为接口为引用类型
        /// </summary>
        /// <param name="parameters"></param>
        [InterfaceComponent("DataMonitor", "数据监测单元", InterfaceComponentCategory.Display,
            Description = "虚拟通道数据的监测,用于显示每个通道的数据详情",
            Icon = )]
        public ProjectDataMonitorComponentPageViewModel(List<DataMonitorSettingItemParamModel> parameters)
        {
            Chilldren.Clear();
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
