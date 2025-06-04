using HelixToolkit.Wpf.SharpDX;

namespace MCCS.Common
{
    public static class IntToColor
    {
        /// <summary>
        /// Converts an ARGB integer to a Color object.
        /// </summary>
        /// <param name="argb"></param>
        /// <returns></returns>
        public static System.Windows.Media.Color ToColor(this int argb)
        {
            var a = (byte)((argb >> 24) & 0xFF);
            var r = (byte)((argb >> 16) & 0xFF);
            var g = (byte)((argb >> 8) & 0xFF);
            var b = (byte)(argb & 0xFF);
            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }

        public static int ToInt(this System.Windows.Media.Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }
    }
}
