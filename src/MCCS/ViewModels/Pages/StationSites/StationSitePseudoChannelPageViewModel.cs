using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.Models.Stations.PseudoChannels;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSitePseudoChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSitePseudoChannel";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;

        private long _stationId = -1;

        public StationSitePseudoChannelPageViewModel(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            AddPseudoChannelCommand = new DelegateCommand(ExecuteAddPseudoChannelCommand);
            EditPseudoChannelCommand = new DelegateCommand(ExecuteEditPseudoChannelCommand);
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        public ObservableCollection<PseudoChannelListItemViewModel> Channels { get; } = [];

        #region Command

        public AsyncDelegateCommand LoadCommand { get; }

        public DelegateCommand AddPseudoChannelCommand { get; }

        public DelegateCommand EditPseudoChannelCommand { get; } 
        #endregion

        #region Private Method 
        private void ExecuteAddPseudoChannelCommand()
        {

        }

        private void ExecuteEditPseudoChannelCommand()
        {

        }

        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1) throw new InvalidOperationException("StationId is not set.");
            Channels.Clear();
            // var pseudoChannels = await _stationSiteAggregateRepository.
        } 
        #endregion
    }
}
