using System.Collections.ObjectModel;
using MCCS.Models.MethodManager;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodMainPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodMainPage";

        public MethodMainPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        #region Property
        public ObservableCollection<MethodItemViewModel> Methods { get; set; }
        #endregion

        #region Command
        #endregion
    }
}
