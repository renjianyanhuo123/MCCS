using System.Collections.ObjectModel;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    /// <summary>
    /// 控制操作组件
    /// </summary>
    public class ControlOperationComponentPageViewModel : BindableBase
    {
        public ControlOperationComponentPageViewModel()
        {
        }

        #region Property 
        public ObservableCollection<ControlUnitComponent> ControlUnits { get; } = []; 
        #endregion
    }
}
