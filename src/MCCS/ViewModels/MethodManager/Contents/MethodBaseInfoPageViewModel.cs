using MCCS.Common.Resources.ViewModels;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Repositories.Method;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodBaseInfoPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodBaseInfoPage";

        private long _methodId = -1;

        private readonly IMethodRepository _methodRepository;

        public MethodBaseInfoPageViewModel(IEventAggregator eventAggregator, 
            IMethodRepository methodRepository) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }
        //public override bool IsNavigationTarget(NavigationContext navigationContext) => false;

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
        /// 方法类型
        /// </summary>
        private MethodTypeEnum _methodType;
        public MethodTypeEnum MethodType
        {
            get => _methodType;
            set => SetProperty(ref _methodType, value);
        }

        /// <summary>
        /// 试验类型
        /// </summary>
        private TestTypeEnum _testType;
        public TestTypeEnum TestType
        {
            get => _testType;
            set => SetProperty(ref _testType, value);
        }
        /// <summary>
        /// 方法标准
        /// </summary> 
        private string _standard = string.Empty ;
        public string Standard
        {
            get => _standard;
            set => SetProperty(ref _standard, value);
        }

        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        } 

        private string _remark = string.Empty;
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; }
        #endregion

        #region Private Method
        private async Task ExecuteLoadCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");
            var methodInfo = await _methodRepository.GetMethodAsync(_methodId);
            Name = methodInfo.Name;
            Code = methodInfo.Code;
            MethodType = methodInfo.MethodType;
            TestType = methodInfo.TestType;
            Standard = methodInfo.Standard;
            Remark = methodInfo.Remark ?? "";
        }
        #endregion

    }
}
