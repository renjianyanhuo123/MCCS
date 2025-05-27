
using System.Collections.ObjectModel;
using MCCS.ViewModels.Others;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService)
        : BaseViewModel(eventAggregator, dialogService)
    {
        public const string Tag = "TestStartPage";

        #region private field
        private ObservableCollection<Model3DViewModel> _model3DList;

        #endregion

        #region Property  
        public ObservableCollection<Model3DViewModel> Model3DList
        {
            get => _model3DList;
            set => SetProperty(ref _model3DList, value);
        }

        #endregion

    }
}
