
using MCCS.Core.Models.TestInfo;
using MCCS.Core.Repositories;
using MCCS.Events;
using MCCS.ViewModels.Others;

namespace MCCS.ViewModels.Pages
{
    public class HomePageViewModel : BaseViewModel
    {
        public const string Tag = "HomePage";
        public HomePageViewModel(
            ITestInfoRepository testInfoRepository,
            IEventAggregator eventAggregator, 
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _testList = testInfoRepository.GetTests(c => c.IsDeleted == false)
                .Select(s => new TestViewModel 
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Standard = s.Standard,
                    Person = s.Person,
                    FilePath = s.FilePath,
                    Status = s.Status,
                    StartTime = s.StartTime?.UtcDateTime,
                    EndTime = s.EndTime?.UtcDateTime,
                    CreateTime = s.CreateTime.UtcDateTime,
                }).ToList();
        }

        #region 页面属性
        private List<TestViewModel> _testList;
        public List<TestViewModel> TestList
        {
            get => _testList;
            set => SetProperty(ref _testList, value);
        }
        #endregion

        #region 命令
        public DelegateCommand<TestViewModel> TestOperationCommand => new(ExcuateTestOperationCommand);
        #endregion

        #region 私有方法
        private void ExcuateTestOperationCommand(TestViewModel param) 
        {
            _eventAggregator.GetEvent<OpenTestOperationEvent>().Publish(new OpenTestOperationEventParam 
            {
                Status = param.Status,
                TestName = param.Name,
                TabId = HomeTestOperationPageViewModel.Tag
            });
        }
        #endregion
    }
}
