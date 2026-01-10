namespace MCCS.Station.Abstractions.Enums
{
    /// <summary>
    /// 段停止原因
    /// </summary>
    public enum StopReasonEnum
    {
        None,
        ReachedTarget,      // 到达目标
        LimitTripped,       // 触发限位
        InterlockTripped,   // 触发联锁
        Timeout,            // 超时
        UserAbort,          // 用户终止
        SensorFault,        // 传感器故障
        SyncError,          // 同步误差
        ControllerError     // 控制器错误
    }
}
