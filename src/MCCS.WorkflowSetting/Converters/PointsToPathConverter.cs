using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MCCS.WorkflowSetting.Converters
{
    public class PointsToPathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not IList<Point> pts || pts.Count == 0) return Geometry.Empty;
            var pg = new PathGeometry();
            var pf = new PathFigure { StartPoint = pts[0], IsClosed = false };
            for (var i = 1; i < pts.Count; i++) pf.Segments.Add(new LineSegment(pts[i], true));
            pg.Figures.Add(pf);
            return pg;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
