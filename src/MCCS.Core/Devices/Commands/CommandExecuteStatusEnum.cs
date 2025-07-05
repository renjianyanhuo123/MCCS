namespace MCCS.Core.Devices.Commands
{
    public enum CommandExecuteStatusEnum : int
    {
        /// <summary>
        /// 暂停
        /// </summary>
        Stoping,
        /// <summary>
        /// 执行中
        /// </summary>
        Executing,
        /// <summary>
        /// 未执行
        /// </summary>
        NoExecute,
        /// <summary>
        /// 执行完成
        /// </summary>
        ExecuttionCompleted
    }
}
