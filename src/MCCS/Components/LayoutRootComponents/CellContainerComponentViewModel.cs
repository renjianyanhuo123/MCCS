using System.Windows;

using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.ViewModels.Dialogs.Project;
using MCCS.ViewModels.ProjectManager.Components;

namespace MCCS.Components.LayoutRootComponents
{
    public sealed class CellContainerComponentViewModel : LayoutNode
    {
        private readonly IDialogService _dialogService;
        public readonly IRegionManager ScopedRegionManager;

        public CellContainerComponentViewModel(IRegionManager regionManager, IDialogService dialogService)
        {
            _dialogService = dialogService;
            ScopedRegionManager = regionManager.CreateRegionManager();
            PopupCommand = new DelegateCommand(ExecutePopupCommand);
        }

        #region Command
        public DelegateCommand PopupCommand { get; }
        #endregion

        #region Private Method
        private void ExecutePopupCommand()
        {
            var parameters = new DialogParameters
            {
                { "ContentViewModel", this },
                { "RegionManager", ScopedRegionManager },
                { "Title", "弹窗标题"}
            };

            _dialogService.Show(nameof(ProjectContentDialogViewModel), parameters, res => { }); 
        } 
        #endregion
    } 
}
