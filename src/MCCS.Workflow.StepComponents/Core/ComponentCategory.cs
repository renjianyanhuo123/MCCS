namespace MCCS.Workflow.StepComponents.Core
{
    /// <summary>
    /// 组件分类
    /// </summary>
    public enum ComponentCategory
    {
        /// <summary>
        /// 通用操作
        /// </summary>
        General,

        /// <summary>
        /// 数据处理
        /// </summary>
        DataProcessing,

        /// <summary>
        /// 流程控制
        /// </summary>
        FlowControl,

        /// <summary>
        /// 用户交互
        /// </summary>
        UserInteraction,

        /// <summary>
        /// 系统操作
        /// </summary>
        System,

        // ========== 结构试验专用分类 ==========

        /// <summary>
        /// 基础与安全 - 配方加载、设备连接、安全联锁、使能控制
        /// </summary>
        SafetyAndSetup,

        /// <summary>
        /// 校准与核查 - 力链校准、引伸计核查
        /// </summary>
        CalibrationAndVerification,

        /// <summary>
        /// 人工操作 - 试件安装、人工确认、清零
        /// </summary>
        ManualOperation,

        /// <summary>
        /// 控制执行 - 段执行、循环执行、预载、卸载
        /// </summary>
        ControlExecution,

        /// <summary>
        /// 数据与报告 - 采集、停止准则、报告生成
        /// </summary>
        DataAndReport
    }
}
