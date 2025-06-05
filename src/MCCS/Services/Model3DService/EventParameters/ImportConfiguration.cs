namespace MCCS.Services.Model3DService.EventParameters
{
    /// <summary>
    /// 导入配置数据模型
    /// </summary>
    public class ImportConfiguration
    {
        public bool EnableAnimations { get; set; } = true;
        public bool EnableEnvironmentMap { get; set; } = true;
        public bool ShowProgressDialog { get; set; } = true;
        public int MaxConcurrentImports { get; set; } = 4;
        public bool AutoFocusCamera { get; set; } = true;
        public bool OptimizeAfterImport { get; set; } = true;
    }
}
