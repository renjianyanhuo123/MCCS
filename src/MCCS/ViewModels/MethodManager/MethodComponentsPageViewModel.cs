using System.Collections.ObjectModel;

using MCCS.Common.Resources.ViewModels;
using MCCS.Events.Mehtod.DynamicGridOperationEvents;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Models.MethodManager.InterfaceSettings;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodComponentsPageViewModel : BaseViewModel
    {
        private readonly IMethodRepository _methodRepository;
        private string _sourceId = "";

        public MethodComponentsPageViewModel(IEventAggregator eventAggregator, IMethodRepository methodRepository) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
            SelectComponentCommand = new DelegateCommand<UiComponentListItemModel>(ExecuteSelectComponentCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var sourceId = navigationContext.Parameters.GetValue<string>("SourceId");
            _sourceId = sourceId;
        }

        #region Command
        public AsyncDelegateCommand LoadCommand { get; }

        public DelegateCommand<UiComponentListItemModel> SelectComponentCommand { get; }
        #endregion

        #region Private Method
        private async Task ExecuteLoadCommand()
        {
            UiComponents.Clear();
            var components = await _methodRepository.GetUiComponentsAsync();
            UiComponents.AddRange(components.Select(s => new UiComponentListItemModel
            {
                NodeId = s.Id,
                Title = s.Title,
                IconStr = s.Icon ?? ""
            }));
        }

        private void ExecuteSelectComponentCommand(UiComponentListItemModel param)
        {
            if (_sourceId == string.Empty) return;
            _eventAggregator.GetEvent<SelectedComponentEvent>().Publish(new SelectedComponentEventParam
            {
                SourceId = _sourceId,
                NodeId = param.NodeId
            });
        }
        #endregion

        #region Property
        public ObservableCollection<UiComponentListItemModel> UiComponents { get; } = [];
        #endregion
    }
}
