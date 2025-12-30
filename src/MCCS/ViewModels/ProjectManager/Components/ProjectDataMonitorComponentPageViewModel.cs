using System.Collections.ObjectModel;

using MCCS.Models.MethodManager.ParamterSettings;
using MCCS.Models.ProjectManager.Components;

namespace MCCS.ViewModels.ProjectManager.Components
{
    public sealed class ProjectDataMonitorComponentPageViewModel : BindableBase
    {
        /// <summary>
        /// 当外部传入List<DataMonitorSettingItemParamModel>  会发生装箱，因为List.GetEnumetor(值类型),然后转换为接口为引用类型
        /// </summary>
        /// <param name="parameters"></param>
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
