using System.ComponentModel;

namespace MCCS.Workflow.StepComponents.Enums
{
    /// <summary>
    /// 组件分类
    /// </summary>
    public enum ComponentCategory
    {
        /// <summary>
        /// 通用操作
        /// </summary>
        [Description("通用操作")]
        General,

        /// <summary>
        /// 数据处理
        /// </summary>
        [Description("数据处理")]
        DataProcessing,

        /// <summary>
        /// 流程控制
        /// </summary>
        [Description("流程控制")]
        FlowControl,

        /// <summary>
        /// 用户交互
        /// </summary>
        [Description("用户交互")] 
        UserInteraction,

        /// <summary>
        /// 系统操作
        /// </summary>
        [Description("系统操作")] 
        System,

        // ========== 结构试验专用分类 ==========

        /// <summary>
        /// 基础与安全 - 配方加载、设备连接、安全联锁、使能控制
        /// </summary>
        [Description("基础与安全")] 
        SafetyAndSetup,

        /// <summary>
        /// 校准与核查 - 力链校准、引伸计核查
        /// </summary>
        [Description("校准与核查")] 
        CalibrationAndVerification,

        /// <summary>
        /// 人工操作 - 试件安装、人工确认、清零
        /// </summary>
        [Description("人工操作")] 
        ManualOperation,

        /// <summary>
        /// 控制执行 - 段执行、循环执行、预载、卸载
        /// </summary>
        [Description("控制执行")] 
        ControlExecution,

        /// <summary>
        /// 数据与报告 - 采集、停止准则、报告生成
        /// </summary>
        [Description("数据处理")] 
        DataAndReport
    }
}
