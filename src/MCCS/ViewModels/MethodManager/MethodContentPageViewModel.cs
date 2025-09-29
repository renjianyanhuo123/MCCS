using System.Collections.ObjectModel;
using MCCS.Models.MethodManager;
using MCCS.Models.Stations;
using MCCS.ViewModels.MethodManager.Contents;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodContentPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodContentPage";

        public MethodContentPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            Menus =
            [
                new MethodMenuItemModel { Name = "常规", Id = 1, Url = MethodBaseInfoPageViewModel.Tag},
                new MethodMenuItemModel { Name = "试样", Id = 2 },
                new MethodMenuItemModel { Name = "测量变量", Id = 3 },
                new MethodMenuItemModel { Name = "计算和结果变量", Id = 4 },
                new MethodMenuItemModel { Name = "工作流配置", Id = 5 }
            ];

        }

        #region Property
        public ObservableCollection<MethodMenuItemModel> Menus { get; }

        private MethodMenuItemModel _selectMenuItemModel;
        public MethodMenuItemModel SelectedMenuItem
        {
            get => _selectMenuItemModel;
            set
            {
                if (SetProperty(ref _selectMenuItemModel, value))
                {

                }
            }
        }
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; } 
        #endregion
    }
}
