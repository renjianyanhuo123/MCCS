namespace MCCS.Interface.Components.Core
{
    /// <summary>
    /// 界面组件视图模型基类
    /// </summary>
    public abstract class BaseInterfaceComponentViewModel : BindableBase, IInterfaceComponent
    {
        private bool _isInitialized;
        private bool _isActive;

        /// <summary>
        /// 组件ID
        /// </summary>
        public abstract string ComponentId { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            protected set => SetProperty(ref _isInitialized, value);
        }

        /// <summary>
        /// 是否处于活动状态
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnActiveChanged(value);
                }
            }
        }

        /// <summary>
        /// 刷新命令
        /// </summary>
        public DelegateCommand RefreshCommand { get; }

        protected BaseInterfaceComponentViewModel()
        {
            RefreshCommand = new DelegateCommand(ExecuteRefresh, CanExecuteRefresh);
        }

        #region IInterfaceComponent Implementation

        /// <summary>
        /// 初始化组件
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized) return;

            OnInitialize();
            IsInitialized = true;
        }

        /// <summary>
        /// 初始化组件（带参数）
        /// </summary>
        public virtual void Initialize(object? parameter)
        {
            if (IsInitialized) return;

            OnInitialize(parameter);
            IsInitialized = true;
        }

        /// <summary>
        /// 刷新组件
        /// </summary>
        public virtual void Refresh()
        {
            OnRefresh();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public virtual void Cleanup()
        {
            OnCleanup();
            IsInitialized = false;
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// 初始化时调用（子类重写）
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// 带参数初始化时调用（子类重写）
        /// </summary>
        protected virtual void OnInitialize(object? parameter)
        {
            OnInitialize();
        }

        /// <summary>
        /// 刷新时调用（子类重写）
        /// </summary>
        protected virtual void OnRefresh()
        {
        }

        /// <summary>
        /// 清理时调用（子类重写）
        /// </summary>
        protected virtual void OnCleanup()
        {
        }

        /// <summary>
        /// 活动状态改变时调用
        /// </summary>
        protected virtual void OnActiveChanged(bool isActive)
        {
        }

        #endregion

        #region Command Methods

        private void ExecuteRefresh()
        {
            Refresh();
        }

        private bool CanExecuteRefresh()
        {
            return IsInitialized;
        }

        #endregion
    }

    /// <summary>
    /// 带参数的界面组件视图模型基类
    /// </summary>
    /// <typeparam name="TParameter">参数类型</typeparam>
    public abstract class BaseInterfaceComponentViewModel<TParameter> : BaseInterfaceComponentViewModel, IInterfaceComponent<TParameter>
    {
        private TParameter? _parameter;

        /// <summary>
        /// 组件参数
        /// </summary>
        public TParameter? Parameter
        {
            get => _parameter;
            protected set => SetProperty(ref _parameter, value);
        }

        /// <summary>
        /// 使用指定类型的参数初始化组件
        /// </summary>
        public virtual void Initialize(TParameter parameter)
        {
            if (IsInitialized) return;

            Parameter = parameter;
            OnInitialize(parameter);
            IsInitialized = true;
        }

        /// <summary>
        /// 带参数初始化时调用（子类重写）
        /// </summary>
        protected virtual void OnInitialize(TParameter parameter)
        {
        }

        /// <summary>
        /// 重写基类的带参数初始化方法
        /// </summary>
        protected override void OnInitialize(object? parameter)
        {
            if (parameter is TParameter typedParameter)
            {
                OnInitialize(typedParameter);
            }
            else
            {
                base.OnInitialize(parameter);
            }
        }
    }
}
