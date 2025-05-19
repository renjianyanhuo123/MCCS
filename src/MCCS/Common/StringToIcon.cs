using MaterialDesignThemes.Wpf;

namespace MCCS.Common
{
    public class StringToIcon
    {
        public static PackIcon ConvertToIcon(string iconStr)
        {
            if (string.IsNullOrEmpty(iconStr)) return new PackIcon();
            return Enum.TryParse<PackIconKind>(iconStr, out var result) ? new PackIcon() { Kind = result} : new PackIcon() { Kind = PackIconKind.Home };
        }
    }
}
