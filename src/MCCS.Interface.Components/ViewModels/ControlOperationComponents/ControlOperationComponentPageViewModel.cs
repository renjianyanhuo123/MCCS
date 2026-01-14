using MCCS.Common.Resources.ViewModels;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    /// <summary>
    /// 控制操作组件
    /// </summary>
    public class ControlOperationComponentPageViewModel:BaseViewModel
    {
        public ControlOperationComponentPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public ControlOperationComponentPageViewModel(IEventAggregator eventAggregator, IDialogService? dialogService) : base(eventAggregator, dialogService)
        {
        }
    }
}
