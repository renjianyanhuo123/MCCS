using MCCS.Models.Stations;
using System.Collections.ObjectModel;
using MCCS.Core.Repositories;

namespace MCCS.ViewModels.Pages.StationSites
{
    public sealed class StationSiteControlChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteControlChannel";

        private long _stationId = -1;
        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;

        public StationSiteControlChannelPageViewModel(IEventAggregator eventAggregator,
            IStationSiteAggregateRepository stationSiteAggregateRepository) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        #region Property 
        public ObservableCollection<ControlChannelHardwareListItemViewModel> HardwareListItems { get; set; }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand => new(ExecuteLoadCommand);
        #endregion

        #region Private Method
        private async Task ExecuteLoadCommand()
        {
            // Load control channel hardware list items from data source
            if(_stationId == -1) throw new InvalidOperationException("StationId is not set.");
            var allDevices = await _stationSiteAggregateRepository.GetStationSiteDevices(_stationId); 
            foreach (var device in allDevices)
            {
                
            }
        }
        #endregion
    }
}
