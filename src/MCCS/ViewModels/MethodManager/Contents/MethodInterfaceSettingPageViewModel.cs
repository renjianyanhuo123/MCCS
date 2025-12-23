using MCCS.Components.LayoutRootComponents;
using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Repositories.Method;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodInterfaceSettingPageViewModel : BaseViewModel
    { 
        private long _methodId = -1;
        private readonly IMethodRepository _methodRepository;

        public MethodInterfaceSettingPageViewModel(
            IMethodRepository methodRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");

        #region Private Method 
        private async Task ExecuteCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException(nameof(_methodId));
            var settingModel = await _methodRepository.GetInterfaceSettingAsync(_methodId);
            if (settingModel == null)
            {
                LayoutRootViewModel = new LayoutRootViewModel(new CellEditableComponentViewModel(_eventAggregator), _eventAggregator);
            }
        }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand { get; }
        #endregion

        #region Property 
        private LayoutRootViewModel _layoutRootViewModel;
        public LayoutRootViewModel LayoutRootViewModel
        {
            get => _layoutRootViewModel;
            set => SetProperty(ref _layoutRootViewModel, value);
        }
        #endregion
    }
}
