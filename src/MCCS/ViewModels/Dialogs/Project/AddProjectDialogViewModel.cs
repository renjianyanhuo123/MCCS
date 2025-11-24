using MaterialDesignThemes.Wpf;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Events.Project;
using MCCS.Infrastructure.Models.ProjectManager;
using MCCS.Infrastructure.Repositories.Project;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Dialogs.Project
{
    public class AddProjectDialogViewModel : BaseViewModel
    {
        public const string Tag = "AddProjectDialog";

        private readonly IProjectRepository _projectRepository;
        private readonly INotificationService _notificationService;

        public AddProjectDialogViewModel(IEventAggregator eventAggregator,
            IProjectRepository projectRepository,
            INotificationService notificationService) : base(eventAggregator)
        {
            _projectRepository = projectRepository;
            _notificationService = notificationService;
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            SaveCommand = new AsyncDelegateCommand(ExecuteSaveCommand);
        }

        #region Property
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _methodName = string.Empty;
        public string MethodName
        {
            get => _methodName;
            set => SetProperty(ref _methodName, value);
        }

        private string _standard = string.Empty;
        public string Standard
        {
            get => _standard;
            set => SetProperty(ref _standard, value);
        }

        private string _person = string.Empty;
        public string Person
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }

        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private long _testTime;
        public long TestTime
        {
            get => _testTime;
            set => SetProperty(ref _testTime, value);
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private string _remark = string.Empty;
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }
        #endregion

        #region Command
        public DelegateCommand CloseCommand { get; }
        public AsyncDelegateCommand SaveCommand { get; }
        #endregion

        #region Private Method
        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }

        private async Task ExecuteSaveCommand()
        {
            var addModel = new ProjectModel
            {
                Name = Name,
                Code = Code,
                Standard = Standard,
                MethodId = 0,
                MethodName = MethodName,
                Person = Person,
                FilePath = FilePath,
                TestTime = TestTime,
                StartTime = StartTime.HasValue ? new DateTimeOffset(StartTime.Value).ToUnixTimeMilliseconds() : 0,
                Remark = Remark
            };
            var addId = await _projectRepository.AddProjectAsync(addModel);
            if (addId > 0)
            {
                DialogHost.Close("RootDialog", true);
                _notificationService.Show("添加成功", "添加项目成功!");
                _eventAggregator.GetEvent<NotificationAddProjectEvent>().Publish(new NotificationAddProjectEventParam
                {
                    ProjectId = addId
                });
            }
            else
            {
                _notificationService.Show("添加失败", "添加项目失败!", NotificationType.Error);
            }
        }
        #endregion
    }
}
