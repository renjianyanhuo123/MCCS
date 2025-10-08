using System.Collections.ObjectModel;
using System.Linq.Expressions;
using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.MethodManager;
using MCCS.Core.Repositories.Method;
using MCCS.Events.Common;
using MCCS.Events.Mehtod;
using MCCS.Models.MethodManager;
using MCCS.UserControl.Params;
using MCCS.Views.Dialogs.Common;
using MCCS.Views.Dialogs.Method;
using Serilog;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodMainPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodMainPage";
         
        private readonly IContainerProvider _containerProvider;
        private readonly IMethodRepository _methodRepository;
        private readonly IRegionManager _regionManager;

        public MethodMainPageViewModel(IEventAggregator eventAggregator,
            IContainerProvider containerProvider,
            IMethodRepository methodRepository,
            IRegionManager regionManager) : base(eventAggregator)
        {
            _containerProvider = containerProvider;
            _methodRepository = methodRepository;
            _regionManager = regionManager;
            _eventAggregator.GetEvent<NotificationAddMethodEvent>().Subscribe(async void (param) =>
            {
                try
                {
                    await SearchData();
                }
                catch (Exception e)
                {
                    Log.Error("添加方法后刷新失败！");
                }
            });
            AddMethodCommand = new AsyncDelegateCommand(ExecuteAddMethodCommand);
            LoadCommand = new AsyncDelegateCommand(SearchData);
            SearchCommand = new AsyncDelegateCommand(SearchData);
            PageChangedCommand = new AsyncDelegateCommand<object?>(OnPageChangedCommand);
            DeleteMethodCommand = new AsyncDelegateCommand<long>(ExecuteDeleteMethodCommand);
            EditMethodCommand = new DelegateCommand<object>(ExecuteEditMethodCommand);
        }

        #region Property  
        private int _pageIndex = 1;  
        private int _pageSize = 10;  

        private long _totalCount;
        public long TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }


        private DateTime? _startTime; 
        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private DateTime? _endTime; 
        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private string? _methodName;
        public string? MethodName
        {
            get => _methodName;
            set => SetProperty(ref _methodName, value);
        }
        public ObservableCollection<MethodItemViewModel> Methods { get; } = [];
        #endregion

        #region Command 
        public AsyncDelegateCommand AddMethodCommand { get; }
        public AsyncDelegateCommand LoadCommand { get; }
        public AsyncDelegateCommand SearchCommand { get; }
        public AsyncDelegateCommand<object?> PageChangedCommand { get; }
        public AsyncDelegateCommand<long> DeleteMethodCommand { get; }
        public DelegateCommand<object> EditMethodCommand { get; }

        #endregion

        #region Private Method

        private static string MethodTypeToString(MethodTypeEnum methodType)
        {
            return methodType switch
            {
                MethodTypeEnum.Fatigue => "疲劳试验",
                _ => "未知类型"
            };
        }

        private static string TestTypeToString(TestTypeEnum testType)
        {
            return testType switch
            {
                TestTypeEnum.Dynamic => "动态试验",
                TestTypeEnum.Static => "静态试验",
                _ => "未知类型"
            };
        }

        private async Task ExecuteAddMethodCommand()
        {
            var addMethodDialog = _containerProvider.Resolve<AddMethodDialog>();
            var result = await DialogHost.Show(addMethodDialog, "RootDialog");
        } 

        private async Task SearchData()
        {
            Methods.Clear();
            Expression<Func<MethodModel, bool>> expression = c => c.IsDeleted == false;
            if (!string.IsNullOrEmpty(MethodName))
            {
                expression = expression.And(c => c.Name.Contains(MethodName));
            }
            if (StartTime != null)
            {
                expression = expression.And(c => c.CreateTime >= StartTime);
            } 
            if (EndTime != null)
            {
                expression = expression.And(c => c.CreateTime <= EndTime);
            } 
            var res = await _methodRepository.GetPageMethodsAsync(_pageIndex, _pageSize, expression);
            TotalCount = res.TotalCount;
            foreach (var m in res.Items)
            {
                Methods.Add(new MethodItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    MethodType = MethodTypeToString(m.MethodType),
                    TestType = TestTypeToString(m.TestType),
                    Standard = m.Standard,
                    FilePath = m.FilePath,
                    CreateTime = m.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdateTime = m.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        private void ExecuteEditMethodCommand(object param)
        {
            if (param is long methodId)
            {
                var temp = new NavigationParameters { { "MethodId", methodId } };
                _regionManager.RequestNavigate(GlobalConstant.MainContentRegionName,
                    new Uri(MethodContentPageViewModel.Tag, UriKind.Relative), temp);
            }
        }

        private async Task ExecuteDeleteMethodCommand(long id)
        {
            var dialog = _containerProvider.Resolve<DeleteConfirmDialog>();
            var result = await DialogHost.Show(dialog, "RootDialog");
            if (result is DialogConfirmEvent { IsConfirmed: true })
            {
                var success = await _methodRepository.DeleteMethodAsync(id);
                if(success) await SearchData();
            }
        }

        private async Task OnPageChangedCommand(object? param)
        { 
            if (param is PageChangedParam temp)
            {
                _pageIndex = temp.CurrentPage;
                _pageSize = temp.PageSize;
                await SearchData();
            }
        }
        #endregion
    }
}
