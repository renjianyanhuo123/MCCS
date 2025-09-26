namespace MCCS.ViewModels.ProjectManager
{
    public sealed class ProjectMainPageViewModel : BaseViewModel
    {
        public const string Tag = "ProjectMainPage";

        public ProjectMainPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
