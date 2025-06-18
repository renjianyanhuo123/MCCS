using Prism.Events;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewStaticControlViewModel : BaseViewModel
    {
        public const string Tag = "StaticControl";

        private int _selectedControlUnitType = 0;

        public ViewStaticControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        #region 页面属性
        public int SelectedControlUnitType
        {
            get => _selectedControlUnitType;
            set => SetProperty(ref _selectedControlUnitType, value);
        }
        #endregion
    }
}
