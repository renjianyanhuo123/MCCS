using MCCS.ViewModels.ProjectManager.Components;

namespace MCCS.ViewModels.Dialogs.Project
{
    public class ProjectContentDialogViewModel : BaseDialog
    {
        private object? _innerViewModel; 
        public object? InnerViewModel
        {
            get => _innerViewModel; 
            set => SetProperty(ref _innerViewModel, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var scopeRegionManager = parameters.GetValue<IRegionManager>("RegionManager");
            scopeRegionManager.RequestNavigate("ProjectContentRegion", new Uri(nameof(ProjectChartComponentPageViewModel), UriKind.Relative));
            Title = parameters.GetValue<string>("Title");
        }
    }
}
