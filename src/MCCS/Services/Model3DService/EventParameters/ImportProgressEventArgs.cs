namespace MCCS.Services.Model3DService.EventParameters
{
    /// <summary>
    /// 作用：报告整体导入进度
    /// 时机：每完成一个文件后更新进度
    /// 频率：与ModelImported相同，但专注于进度信息
    /// 用途：更新进度条和状态显示
    /// </summary>
    public class ImportProgressEventArgs : EventArgs
    {
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
        public string CurrentFileName { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
