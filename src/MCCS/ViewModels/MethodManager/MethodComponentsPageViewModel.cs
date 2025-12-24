using System.Collections.ObjectModel;

using MCCS.Infrastructure.Repositories.Method;
using MCCS.Models.MethodManager.InterfaceSettings;

namespace MCCS.ViewModels.MethodManager
{
    public class MethodComponentsPageViewModel : BaseViewModel
    {
        private readonly IMethodRepository _methodRepository;

        public MethodComponentsPageViewModel(IEventAggregator eventAggregator, IMethodRepository methodRepository) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
            SelectComponentCommand = new DelegateCommand<UiComponentListItemModel>(ExecuteSelectComponentCommand);
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
            // _eventAggregator.GetEvent<>()
        }
        #endregion

        #region Property
        public ObservableCollection<UiComponentListItemModel> UiComponents { get; } = [];
        #endregion
    }
}
