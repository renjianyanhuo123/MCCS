namespace MCCS.Workflow.StepComponents.Models
{
    /// <summary>
    /// 控制模式
    /// </summary>
    public enum ControlMode
    {
        /// <summary>
        /// 力控制
        /// </summary>
        Force,

        /// <summary>
        /// 位移控制
        /// </summary>
        Displacement,

        /// <summary>
        /// 应变控制
        /// </summary>
        Strain,

        /// <summary>
        /// 空闲模式
        /// </summary>
        Idle
    }

    /// <summary>
    /// 波形类型
    /// </summary>
    public enum WaveformType
    {
        /// <summary>
        /// 斜坡
        /// </summary>
        Ramp,

        /// <summary>
        /// 正弦
        /// </summary>
        Sine,

        /// <summary>
        /// 三角波
        /// </summary>
        Triangle,

        /// <summary>
        /// 方波
        /// </summary>
        Square,

        /// <summary>
        /// 自定义点列
        /// </summary>
        CustomPoints
    }

    /// <summary>
    /// 试验类型
    /// </summary>
    public enum TestType
    {
        /// <summary>
        /// 单调静力
        /// </summary>
        StaticMonotonic,

        /// <summary>
        /// 拟静力低周反复
        /// </summary>
        CyclicQuasiStatic,

        /// <summary>
        /// 疲劳 S-N
        /// </summary>
        FatigueSN,

        /// <summary>
        /// 蠕变/松弛
        /// </summary>
        CreepRelax,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }

    /// <summary>
    /// 步骤结果代码
    /// </summary>
    public enum StepResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        OK,

        /// <summary>
        /// 警告但可继续
        /// </summary>
        Warning,

        /// <summary>
        /// 需要用户操作
        /// </summary>
        NeedUserAction,

        /// <summary>
        /// 终止
        /// </summary>
        Abort
    }
}
