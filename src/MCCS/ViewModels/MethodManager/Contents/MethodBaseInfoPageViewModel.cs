using MCCS.Common.DataManagers;
using MCCS.Core.Models.MethodManager;
using MCCS.Core.Repositories.Method;

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
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }

        #region Property
        /// <summary>
        /// 方法名称
        /// </summary> 
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    if (GlobalDataManager.Instance.MethodInfo?.MethodBaseInfo == null) return;
                    GlobalDataManager.Instance.MethodInfo.MethodBaseInfo.Name = _name;
                }
            }
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
        private string _standard;
        public string Standard
        {
            get => _standard;
            set
            {
                if (SetProperty(ref _standard, value))
                {
                    if (GlobalDataManager.Instance.MethodInfo?.MethodBaseInfo == null) return;
                    GlobalDataManager.Instance.MethodInfo.MethodBaseInfo.Standard = _standard;
                }
            }
        }

        private string _code;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        } 

        private string _remark;
        public string Remark
        {
            get => _remark;
            set
            {
                if (SetProperty(ref _remark, value))
                {
                    if (GlobalDataManager.Instance.MethodInfo?.MethodBaseInfo == null) return;
                    GlobalDataManager.Instance.MethodInfo.MethodBaseInfo.Remark = _remark;
                }
            }
        }
        #endregion

        #region Command 
        public DelegateCommand LoadCommand { get; }
        #endregion

        #region Private Method
        private void ExecuteLoadCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");
            var methodInfo = GlobalDataManager.Instance.MethodInfo;
            if (methodInfo?.MethodBaseInfo == null) return;
            Name = methodInfo.MethodBaseInfo.Name;
            Code = methodInfo.MethodBaseInfo.Code;
            MethodType = methodInfo.MethodBaseInfo.MethodType;
            TestType = methodInfo.MethodBaseInfo.TestType;
            Standard = methodInfo.MethodBaseInfo.Standard;
            Remark = methodInfo.MethodBaseInfo.Remark;
        }
        #endregion

    }
}
