using System.Collections.ObjectModel;
using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.Methods;
using MCCS.Core.Helper;
using MCCS.Core.Repositories.Method;
using MCCS.Models.MethodManager;
using MCCS.ViewModels.MethodManager.Contents;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodContentPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodContentPage";

        private long _methodId = -1;

        private readonly IRegionManager _regionManager;
        private readonly IMethodRepository _methodRepository;

        public MethodContentPageViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager,
            IMethodRepository methodRepository) : base(eventAggregator)
        {
            Menus =
            [
                new MethodMenuItemModel { Name = "常规", Id = 1, Url = MethodBaseInfoPageViewModel.Tag},
                new MethodMenuItemModel { Name = "试样", Id = 2 },
                new MethodMenuItemModel { Name = "测量变量", Id = 3 },
                new MethodMenuItemModel { Name = "计算和结果变量", Id = 4 },
                new MethodMenuItemModel { Name = "工作流配置", Id = 5 }
            ];
            _regionManager = regionManager;
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExexuteLoadCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }

        #region Property
        public ObservableCollection<MethodMenuItemModel> Menus { get; }

        private MethodMenuItemModel? _selectMenuItemModel;
        public MethodMenuItemModel? SelectedMenuItem
        {
            get => _selectMenuItemModel;
            set
            {
                if (SetProperty(ref _selectMenuItemModel, value))
                {
                    if (_selectMenuItemModel == null) return;
                    var temp = new NavigationParameters { { "MethodId", _methodId } };
                    _regionManager.RequestNavigate(GlobalConstant.MethodNavigateRegionName, new Uri(_selectMenuItemModel.Url, UriKind.Relative), temp);
                }
            }
        }
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; }
        #endregion

        #region Private Method 
        private async Task ExexuteLoadCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");
            var methodBaseInfo = await _methodRepository.GetMethodAsync(_methodId);
            if (methodBaseInfo == null) throw new ArgumentNullException("methodBaseInfo is null");
            var methodInfo = new MethodContentItemModel(_methodId);
            methodInfo.SetBaseInfo(new MethodBaseInfo(
                methodBaseInfo.Name,
                methodBaseInfo.MethodType,
                methodBaseInfo.TestType,
                methodBaseInfo.Standard,
                methodBaseInfo.Code,
                methodBaseInfo.Remark));
            if (FileHelper.FileExists(methodBaseInfo.FilePath))
            {
                var temp = await FileHelper.ReadJsonAsync<MethodContentItemModel>(methodBaseInfo.FilePath);
                if (temp == null) throw new ArgumentNullException("methodInfo is null"); 
            }
            GlobalDataManager.Instance.SetValue(methodInfo);
            SelectedMenuItem = Menus.FirstOrDefault(c => c.Id == 1); 
        }

        #endregion
    }
}
