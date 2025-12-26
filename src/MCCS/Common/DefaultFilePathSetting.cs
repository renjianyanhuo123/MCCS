namespace MCCS.Common
{
    public static class DefaultFilePathSetting
    {
        private static readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;

        public static string DefaultMethodSavePath = $"{_basePath}//MethodInfoFiles//";
    }
}
