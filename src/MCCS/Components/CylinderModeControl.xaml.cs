using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCCS.Components
{
    /// <summary>
    /// CylinderModeControl.xaml 的交互逻辑
    /// </summary>
    public partial class CylinderModeControl : UserControl
    {
        //public CylinderModeControl()
        //{
        //    InitializeComponent();
        //    // 创建 Path
        //    Path cylinderSide = new Path
        //    {
        //        Stroke = Brushes.Black,
        //        StrokeThickness = 2
        //    };

        //    // 设置渐变填充
        //    LinearGradientBrush metalBrush = new LinearGradientBrush
        //    {
        //        StartPoint = new Point(0, 0),
        //        EndPoint = new Point(1, 0)
        //    };
        //    metalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BBBBBB"), 0.00));
        //    metalBrush.GradientStops.Add(new GradientStop(Colors.White, 0.20));
        //    metalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#AAAAAA"), 0.40));
        //    metalBrush.GradientStops.Add(new GradientStop(Colors.White, 0.60));
        //    metalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BBBBBB"), 1.00));

        //    cylinderSide.Fill = metalBrush;

        //    // 构建 PathGeometry
        //    PathGeometry pathGeometry = new PathGeometry();
        //    PathFigure pathFigure = new PathFigure { StartPoint = new Point(100, 75) };

        //    // 上弧
        //    ArcSegment topArc = new ArcSegment
        //    {
        //        Point = new Point(300, 75),
        //        Size = new Size(100, 25),
        //        SweepDirection = SweepDirection.Clockwise
        //    };
        //    pathFigure.Segments.Add(topArc);

        //    // 右边竖线
        //    pathFigure.Segments.Add(new LineSegment(new Point(300, 200), true));

        //    // 下弧
        //    ArcSegment bottomArc = new ArcSegment
        //    {
        //        Point = new Point(100, 200),
        //        Size = new Size(100, 25),
        //        SweepDirection = SweepDirection.Clockwise
        //    };
        //    pathFigure.Segments.Add(bottomArc);

        //    // 左边竖线
        //    pathFigure.Segments.Add(new LineSegment(new Point(100, 75), true));

        //    pathGeometry.Figures.Add(pathFigure);
        //    cylinderSide.Data = pathGeometry;

        //    // 加入 Canvas
        //    MyCanvas.Children.Add(cylinderSide);
        //}
    }
}
