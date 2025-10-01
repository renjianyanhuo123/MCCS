using System.ComponentModel.Design;

namespace MCCS.ViewModels.Pages.WorkflowSteps
{
    public sealed class WorkflowStepListPageViewModel : BaseViewModel
    {
        public const string Tag = "WorkflowStepListPage";

        public WorkflowStepListPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
