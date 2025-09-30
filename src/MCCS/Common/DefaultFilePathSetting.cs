namespace MCCS.Common
{
    public static class DefaultFilePathSetting
    {
        private static string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        public static string DefaultMethodSavePath = $"{BasePath}//MethodInfoFiles//";
    }
}
