using MaterialDesignThemes.Wpf;
using MCCS.Components.GlobalNotification.Models; 
using MCCS.Events.Mehtod;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Repositories.Method;
using MCCS.Services.NotificationService;

namespace MCCS.ViewModels.Dialogs.Method
{
    public class AddMethodDialogViewModel : BaseViewModel
    {
        public const string Tag = "AddMethodDialog";

        private readonly IMethodRepository _methodRepository;
        private readonly INotificationService _notificationService;

        public AddMethodDialogViewModel(IEventAggregator eventAggregator,
            IMethodRepository methodRepository,
            INotificationService notificationService) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            _notificationService = notificationService;
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            SaveCommand = new AsyncDelegateCommand(ExecuteSaveCommand);
        }

        #region Property
        /// <summary>
        /// 方法名称
        /// </summary> 
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        /// <summary>
        /// 编号
        /// </summary>
        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        /// <summary>
        /// 方法类型
        /// </summary>
        private int _methodType;
        public int MethodType
        {
            get => _methodType;
            set => SetProperty(ref _methodType, value);
        }

        /// <summary>
        /// 试验类型
        /// </summary>
        private int _testType;
        public int TestType
        {
            get => _testType;
            set => SetProperty(ref _testType, value);
        }
        /// <summary>
        /// 方法标准
        /// </summary> 
        private string _standard = string.Empty;
        public string Standard
        {
            get => _standard;
            set => SetProperty(ref _standard, value);
        }

        /// <summary>
        /// 文件路径
        /// </summary> 
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// 备注
        /// </summary>
        private string _remark = string.Empty;
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        #endregion

        #region Command 
        public DelegateCommand CloseCommand { get; }
        public AsyncDelegateCommand SaveCommand { get; }
        #endregion

        #region Private Method
        private void ExecuteCloseCommand()
        {
            DialogHost.Close("RootDialog");
        }

        private async Task ExecuteSaveCommand()
        {
            var addModel = new MethodModel()
            {
                Name = Name,
                Code = Code,
                MethodType = (MethodTypeEnum)MethodType,
                FilePath = FilePath,
                Standard = Standard,
                Remark = Remark,
                TestType = (TestTypeEnum)TestType
            };
            var addId = await _methodRepository.AddMethodAsync(addModel);
            if (addId > 0)
            {
                DialogHost.Close("RootDialog", true);
                _notificationService.Show("添加成功", "添加方法成功!");
                _eventAggregator.GetEvent<NotificationAddMethodEvent>().Publish(new NotificationAddMethodEventParam { MethodId = addId });
            }
            else
            {
                _notificationService.Show("添加失败", "添加方法失败!", NotificationType.Error); 
            }
        }

        #endregion

    }
}
