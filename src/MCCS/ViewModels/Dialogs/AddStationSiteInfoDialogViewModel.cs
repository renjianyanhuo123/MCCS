using MaterialDesignThemes.Wpf;
using MCCS.Core.Models.StationSites;
using MCCS.Core.Repositories;
using MCCS.Events.StationSites;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Dialogs
{
    public class AddStationSiteInfoDialogViewModel : BaseViewModel
    {
        public const string Tag = "AddStationSiteInfo";

        private readonly IStationSiteAggregateRepository _stationSiteRepository;
        private readonly INotificationService _notificationService;

        public AddStationSiteInfoDialogViewModel(IEventAggregator eventAggregator, 
            IStationSiteAggregateRepository stationSiteRepository,
            INotificationService notificationService) : base(eventAggregator)
        {
            _stationSiteRepository = stationSiteRepository;
            _notificationService = notificationService;
        }

        #region Property
        private string _stationName = string.Empty;
        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion

        #region Command 
        public DelegateCommand CloseDialogCommand => new(ExecuteCloseDialogCommand);

        public AsyncDelegateCommand SaveStationCommand => new(ExecuteSaveStationCommand);
        #endregion

        #region Private Method

        private void ExecuteCloseDialogCommand()
        {
             DialogHost.Close("RootDialog");
        }

        private async Task ExecuteSaveStationCommand()
        {
            if (string.IsNullOrEmpty(StationName)) return;
            var addModel = new StationSiteInfo()
            {
                StationName = StationName,
                Description = Description,
                IsUsing = false
            };
            var addId = await _stationSiteRepository.AddStationInfoAsync(addModel);
            if (addId > 0)
            {
                _notificationService.Show("添加成功", "添加站点成功");
                DialogHost.Close("RootDialog");
                _eventAggregator.GetEvent<NotificationAddStationSiteEvent>()
                    .Publish(new NotificationAddStationSiteEventParam
                    {
                        StationId = addId
                    });
            }
        }

        #endregion
    }
}
