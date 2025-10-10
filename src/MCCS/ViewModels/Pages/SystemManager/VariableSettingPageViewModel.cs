using System.Collections.ObjectModel;
using MCCS.Services.NotificationService;
using MCCS.UserControl.Transfer;

namespace MCCS.ViewModels.Pages.SystemManager
{
    public sealed class VariableSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "VariableSetting";

        private readonly INotificationService _notificationService;

        public VariableSettingPageViewModel(
            INotificationService notificationService,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _notificationService = notificationService;
        }
         

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //var variableId = navigationContext.Parameters.GetValue<long>("VariableId"); 
            //var variableInfo = _channelAggregateRepository.GetVariableInfoById(variableId);
            //var channelAggregate = _channelAggregateRepository.GetChannelById(variableInfo.ChannelId) ?? throw new ArgumentNullException(nameof(variableInfo.ChannelId));
            //var variableHardwareIds = channelAggregate.Variables.Where(t => !string.IsNullOrWhiteSpace(t.HardwareInfos))
            //    .SelectMany(t => t.HardwareInfos?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [])
            //    .Select(s => long.Parse(s.Trim()))
            //    .Distinct()
            //    .ToList();
            //var remainDeviceInfos = channelAggregate.DeviceInfos
            //    .Where(c => !variableHardwareIds.Contains(c.Id))
            //    .Select(s => new TransferItemModel
            //    {
            //        Id = s.Id.ToString(),
            //        IsSelected = false,
            //        Name = s.DeviceName
            //    }).ToList(); 
            //var selectedVariableIds = variableInfo.HardwareInfos?.Split(",").Select(long.Parse).ToList() ?? [];
            //var targetDeviceInfos = channelAggregate.DeviceInfos.Where(c => selectedVariableIds.Contains(c.Id))
            //    .Select(s => new TransferItemModel
            //    {
            //        Id = s.Id.ToString(),
            //        IsSelected = false,
            //        Name = s.DeviceName
            //    }).ToList();
            //SourceModels.Clear();
            //foreach (var transferModel in remainDeviceInfos)
            //{
            //    SourceModels.Add(transferModel);
            //}
            //TargetModels.Clear();
            //foreach (var targetDeviceInfo in targetDeviceInfos)
            //{
            //    TargetModels.Add(targetDeviceInfo);
            //} 
            //VariableId = variableId;
            //InternalId = variableInfo.VariableId;
            //VariableName = variableInfo.Name;
            //IsShowable = variableInfo.IsShowable;
            //IsCanCalibrate = variableInfo.IsCanCalibration;
            //IsCanControl = variableInfo.IsCanControl;
            //IsCanSetLimit = variableInfo.IsCanSetLimit; 
        }

        #region Property
        public long VariableId { get; set; }

        private string _variableName = string.Empty;
        public string VariableName
        {
            get => _variableName;
            set => SetProperty(ref _variableName, value);
        }

        private string _internalId = string.Empty;
        public string InternalId
        {
            get => _internalId;
            set => SetProperty(ref _internalId, value);
        }

        private bool _isShowable = false;
        public bool IsShowable
        {
            get => _isShowable;
            set => SetProperty(ref _isShowable, value);
        }

        private bool _isCanControl = false;
        public bool IsCanControl
        {
            get => _isCanControl;
            set => SetProperty(ref _isCanControl, value);
        }

        private bool _isCanCalibrate = false;
        public bool IsCanCalibrate
        {
            get => _isCanCalibrate;
            set => SetProperty(ref _isCanCalibrate, value);
        }

        private bool _isCanSetLimit = false;
        public bool IsCanSetLimit
        {
            get => _isCanSetLimit;
            set => SetProperty(ref _isCanSetLimit, value);
        }

        public ObservableCollection<TransferItemModel> SourceModels { get; private set; } = [];

        public ObservableCollection<TransferItemModel> TargetModels { get; private set; } = [];

        #endregion

        #region Command
        public AsyncDelegateCommand SaveCommand => new(ExecuteSaveCommand);
        #endregion

        #region private method
        private async Task ExecuteSaveCommand()
        {
            //if (string.IsNullOrEmpty(VariableName)) return;
            //var success = await _channelAggregateRepository.UpdateVariableInfoAsync(new VariableInfo
            //{
            //    Id = VariableId,
            //    VariableId = InternalId,
            //    IsCanCalibration = IsCanCalibrate,
            //    IsCanControl = IsCanControl,
            //    IsCanSetLimit = IsCanSetLimit,
            //    Name = VariableName,
            //    HardwareInfos = string.Join(",", TargetModels.Select(s => s.Id).ToList())
            //});
            //if (success)
            //{
            //    _notificationService.Show("保存成功", "变量保存成功！", NotificationType.Success, 3);
            //    _eventAggregator.GetEvent<NotificationUpdateVariableEvent>().Publish(new NotificationUpdateVariableEventParam(VariableId));
            //}
        }
        #endregion
    }
}
