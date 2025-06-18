using MCCS.Models.ControlCommand;

namespace MCCS.ViewModels.Pages.ControlCommandPages
{
    public class ViewFatigueControlViewModel : BaseViewModel
    {
        public const string Tag = "FatigueControl";

        private int _selectedControlUnitType = 0;

        public ViewFatigueControlViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
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
