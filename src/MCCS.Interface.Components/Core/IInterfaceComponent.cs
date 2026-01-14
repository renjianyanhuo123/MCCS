namespace MCCS.Interface.Components.Core
{
    /// <summary>
    /// 界面组件接口 - 定义界面组件的基本行为
    /// </summary>
    public interface IInterfaceComponent
    {
        /// <summary>
        /// 组件ID（对应 InterfaceComponentAttribute 中的 Id）
        /// </summary>
        string ComponentId { get; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Initialize();

        /// <summary>
        /// 初始化组件（带参数）
        /// </summary>
        /// <param name="parameter">初始化参数</param>
        void Initialize(object? parameter);

        /// <summary>
        /// 刷新组件数据
        /// </summary>
        void Refresh();

        /// <summary>
        /// 清理组件资源
        /// </summary>
        void Cleanup();

        /// <summary>
        /// 组件是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 组件是否处于活动状态
        /// </summary>
        bool IsActive { get; set; }
    }

    /// <summary>
    /// 带参数的界面组件接口
    /// </summary>
    /// <typeparam name="TParameter">参数类型</typeparam>
    public interface IInterfaceComponent<in TParameter> : IInterfaceComponent
    {
        /// <summary>
        /// 使用指定类型的参数初始化组件
        /// </summary>
        /// <param name="parameter">初始化参数</param>
        void Initialize(TParameter parameter);
    }
}
