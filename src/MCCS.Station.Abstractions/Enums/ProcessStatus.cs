namespace MCCS.Station.Abstractions.Enums;

/// <summary>
/// 运行流程维度状态
/// 回答：逻辑流程走到哪了？（待机、试验、暂停、结束、回零…）
/// 这是状态机最擅长管理的部分
/// </summary>
public enum ProcessStatus : byte
{
    /// <summary>
    /// 空闲待机
    /// </summary>
    Idle = 0,

    /// <summary>
    /// 已准备可启动（加载完成、参数验证通过）
    /// </summary>
    Armed = 1,

    /// <summary>
    /// 正在预处理（回零、预紧等）
    /// </summary>
    Preparing = 2,

    /// <summary>
    /// 正在执行试验
    /// </summary>
    Running = 3,

    /// <summary>
    /// 暂停中
    /// </summary>
    Paused = 4,

    /// <summary>
    /// 正在停止
    /// </summary>
    Stopping = 5,

    /// <summary>
    /// 试验完成
    /// </summary>
    Completed = 6,

    /// <summary>
    /// 正在回零/卸载
    /// </summary>
    Unloading = 7,

    /// <summary>
    /// 手动控制模式
    /// </summary>
    Manual = 8
}
