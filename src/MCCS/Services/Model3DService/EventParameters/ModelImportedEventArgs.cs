namespace MCCS.Services.Model3DService.EventParameters
{
    /// <summary>
    /// 每当单个模型文件导入完成时触发
    /// 时机：单个文件处理完毕（成功或失败）
    /// 频率：导入N个文件就触发N次
    /// 用途：实时处理每个导入的模型
    /// </summary>
    public class ModelImportedEventArgs : EventArgs
    {
        public ImportResult ImportResult { get; set; }
        public int Index { get; set; }
    }
}
