using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Events.Mehtod.DynamicGridOperationEvents;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Services.ProjectServices;
using MCCS.ViewModels.Dialogs.Project;

using Newtonsoft.Json;

namespace MCCS.Components.LayoutRootComponents
{
    public sealed class CellContainerComponentViewModel : LayoutNode
    {
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;

        public CellContainerComponentViewModel(IDialogService dialogService, 
            IProjectComponentFactoryService projectComponentFactoryService,
            IEventAggregator eventAggregator, 
            CellNode? node = null)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            if(node != null) InnerViewModel = projectComponentFactoryService.BuildComponentViewModel(node);
            PlaceholderPopupCommand = new DelegateCommand(ExecutePlaceholderPopupCommand);
            NonPlaceholderPopupCommand = new DelegateCommand(ExevuteNonPlaceholderPopupCommand);
        }

        #region Property
        private object? _innerViewModel;
        public object? InnerViewModel
        {
            get => _innerViewModel;
            set => SetProperty(ref _innerViewModel, value);
        }
        #endregion

        #region Command
        public DelegateCommand PlaceholderPopupCommand { get; }
        public DelegateCommand NonPlaceholderPopupCommand { get; }
        #endregion

        #region Private Method
        private void ChangeUiComponent()
        {
            if (Parent == null) return;
            var grandFather = Parent.Parent;
            var brotherNode = Parent.LeftNode == this ? Parent.RightNode : Parent.LeftNode;
            if (brotherNode == null) return;
            if (grandFather == null)
            {
                brotherNode.Parent = null;
                _eventAggregator.GetEvent<ChangedRootEvent>().Publish(new ChangedRootEventParam
                {
                    Root = brotherNode
                });
            }
            else
            {
                brotherNode.Parent = grandFather;
                if (Parent == grandFather.LeftNode)
                {
                    grandFather.LeftNode = null;
                    grandFather.LeftNode = brotherNode;
                }
                else
                {
                    grandFather.RightNode = null;
                    grandFather.RightNode = brotherNode;
                }
            }
        }

        private void ExevuteNonPlaceholderPopupCommand()
        {
            if (InnerViewModel == null) return;
            var parameters = new DialogParameters
            {
                { "ContentViewModel", InnerViewModel },
                { "Title", "弹窗标题" },
                { "IsPlaceholderComponent", false }
            };
            ChangeUiComponent();
            _dialogService.Show(nameof(ProjectContentDialogViewModel), parameters, res => { });
        }

        private void ExecutePlaceholderPopupCommand()
        {
            if (InnerViewModel == null) return;
            var parameters = new DialogParameters
            {
                { "ContentViewModel", InnerViewModel },
                { "Title", "弹窗标题" },
                { "IsPlaceholderComponent", true }
            };
            // ChangeUiComponent();
            InnerViewModel = null;
            _dialogService.Show(nameof(ProjectContentDialogViewModel), parameters, res =>
            {
                InnerViewModel = res.Parameters.GetValue<object>("ContentViewModel"); 
            }); 
        } 
        #endregion
    } 
}
