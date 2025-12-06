using System.Collections.ObjectModel;
using System.Linq.Expressions;
using MaterialDesignThemes.Wpf;
using MCCS.Events.Common;
using MCCS.Events.Project;
using MCCS.Infrastructure.Models.ProjectManager;
using MCCS.Infrastructure.Repositories.Project;
using MCCS.Models.ProjectManager;
using MCCS.UserControl.Params;
using MCCS.Views.Dialogs.Common;
using MCCS.Views.Dialogs.Project;
using Serilog;

namespace MCCS.ViewModels.ProjectManager
{
    public sealed class ProjectMainPageViewModel : BaseViewModel
    {
        public const string Tag = "ProjectMainPage";

        private readonly IContainerProvider _containerProvider;
        private readonly IProjectRepository _projectRepository;

        public ProjectMainPageViewModel(IEventAggregator eventAggregator,
            IContainerProvider containerProvider,
            IProjectRepository projectRepository) : base(eventAggregator)
        {
            _containerProvider = containerProvider;
            _projectRepository = projectRepository;
            _eventAggregator.GetEvent<NotificationAddProjectEvent>().Subscribe(async void (_) =>
            {
                try
                {
                    await SearchData();
                }
                catch (Exception e)
                {
                    Log.Error($"添加项目后刷新失败！{e.Message}");
                }
            });
            AddProjectCommand = new AsyncDelegateCommand(ExecuteAddProjectCommand);
            LoadCommand = new AsyncDelegateCommand(SearchData);
            SearchCommand = new AsyncDelegateCommand(SearchData);
            PageChangedCommand = new AsyncDelegateCommand<object?>(OnPageChangedCommand);
            DeleteProjectCommand = new AsyncDelegateCommand<long>(ExecuteDeleteProjectCommand);
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

        private string? _projectName;
        public string? ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        public ObservableCollection<ProjectItemViewModel> Projects { get; } = [];
        #endregion

        #region Command
        public AsyncDelegateCommand AddProjectCommand { get; }
        public AsyncDelegateCommand LoadCommand { get; }
        public AsyncDelegateCommand SearchCommand { get; }
        public AsyncDelegateCommand<object?> PageChangedCommand { get; }
        public AsyncDelegateCommand<long> DeleteProjectCommand { get; }
        #endregion

        #region Private Method
        private async Task ExecuteAddProjectCommand()
        {
            var dialog = _containerProvider.Resolve<AddProjectDialog>();
            await DialogHost.Show(dialog, "RootDialog");
        }

        private async Task SearchData()
        {
            Projects.Clear();
            Expression<Func<ProjectModel, bool>> expression = c => c.IsDeleted == false;
            if (!string.IsNullOrEmpty(ProjectName))
            {
                expression = expression.And(c => c.Name.Contains(ProjectName));
            }
            if (StartTime != null)
            {
                expression = expression.And(c => c.CreateTime >= StartTime);
            }
            if (EndTime != null)
            {
                expression = expression.And(c => c.CreateTime <= EndTime);
            }

            var res = await _projectRepository.GetPageMethodsAsync(_pageIndex, _pageSize, expression);
            TotalCount = res.TotalCount;
            foreach (var project in res.Items)
            {
                var startTime = project.StartTime > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(project.StartTime).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    : string.Empty;
                Projects.Add(new ProjectItemViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    Code = project.Code,
                    MethodName = project.MethodName ?? string.Empty,
                    Standard = project.Standard,
                    Person = project.Person ?? string.Empty,
                    FilePath = project.FilePath,
                    TestTime = project.TestTime.ToString(),
                    StartTime = startTime,
                    Remark = project.Remark ?? string.Empty,
                    CreateTime = project.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdateTime = project.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        private async Task ExecuteDeleteProjectCommand(long id)
        {
            var dialog = _containerProvider.Resolve<DeleteConfirmDialog>();
            var result = await DialogHost.Show(dialog, "RootDialog");
            if (result is DialogConfirmEvent { IsConfirmed: true })
            {
                var success = await _projectRepository.DeleteProjectAsync(id);
                if (success) await SearchData();
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
