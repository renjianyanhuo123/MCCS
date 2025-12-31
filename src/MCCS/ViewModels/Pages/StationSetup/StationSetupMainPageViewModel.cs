using System.Collections.ObjectModel;

using MCCS.Common.Resources.ViewModels;
using MCCS.Models.Stations;

namespace MCCS.ViewModels.Pages.StationSetup
{
    public sealed class StationSetupMainPageViewModel : BaseViewModel
    {
        public StationSetupMainPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        { 
            MenuItems = [
                new StationMenuItemModel { Name = "传感器标定", Id = 1 },
                new StationMenuItemModel { Name = "安全防护参数设置", Id = 2 },
                new StationMenuItemModel { Name = "设备调谐", Id = 3 },
                new StationMenuItemModel { Name = "清零", Id = 4 },
                new StationMenuItemModel { Name = "补偿器", Id = 5 }
            ];
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
        }

        #region Property 
        public ObservableCollection<StationMenuItemModel> MenuItems { get; set; }
         
        private StationMenuItemModel? _selectedMenuItem;
        public StationMenuItemModel? SelectedMenuItem
        {
            get => _selectedMenuItem; 
            set => SetProperty(ref _selectedMenuItem, value);
        }
        #endregion

        #region Command 
        public DelegateCommand LoadCommand { get; }
        #endregion

        #region Private Method
        private void ExecuteLoadCommand()
        {
            SelectedMenuItem = MenuItems[0];
        }
        #endregion
    }
}
