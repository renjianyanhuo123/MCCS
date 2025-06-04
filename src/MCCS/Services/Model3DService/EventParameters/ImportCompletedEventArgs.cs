namespace MCCS.Services.Model3DService.EventParameters
{
    /// <summary>
    /// 作用：整个批量导入操作完成时触发
    /// 时机：所有文件都处理完毕后
    /// 频率：每次批量导入操作只触发一次
    /// 用途：执行收尾工作和最终处理
    /// </summary>
    public class ImportCompletedEventArgs : EventArgs
    {
        public int TotalImported { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
