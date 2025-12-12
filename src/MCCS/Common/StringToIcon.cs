using MaterialDesignThemes.Wpf;

namespace MCCS.Common
{
    public class StringToIcon
    {
        public static PackIconKind ConvertToIcon(string iconStr)
        {
            if (string.IsNullOrEmpty(iconStr)) return PackIconKind.Home;
            return Enum.TryParse<PackIconKind>(iconStr, out var result) ? result : PackIconKind.Home;
        }
    }
}
