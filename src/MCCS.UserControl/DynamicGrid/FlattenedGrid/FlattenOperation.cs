using System.Windows.Controls;

namespace MCCS.UserControl.DynamicGrid.FlattenedGrid;

public readonly record struct RectD(
    double X,
    double Y,
    double Width,
    double Height);

public enum RectKind
{
    Content,
    Splitter
}

public sealed class RectInfo
{
    public RectKind Kind { get; }
    public RectD Rect { get; }

    public CellLayoutNode? Leaf { get; }
    public SplitterNode? Split { get; }

    private RectInfo(RectKind kind, RectD rect)
    {
        Kind = kind;
        Rect = rect;
    }

    public static RectInfo Content(CellLayoutNode leaf, RectD rect) => new(RectKind.Content, rect, leaf, null);

    public static RectInfo Splitter(SplitterNode split, RectD rect) => new(RectKind.Splitter, rect, null, split);

    // 私有 set（防止误用）
    private RectInfo(RectKind kind, RectD rect, CellLayoutNode? leaf, SplitterNode? split)
    {
        Kind = kind;
        Rect = rect;
        Leaf = leaf;
        Split = split;
    }
}


/// <summary>
/// 扁平化操作处理节点
/// </summary>
public static class FlattenOperation
{
    public const double _splitterThickness = 1e-4;

    /// <summary>
    /// 获取所有叶子节点的矩形位置
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int IndexOf(List<double> lines, double value) => lines.FindIndex(x => Math.Abs(x - value) < 1e-6);
    /// <summary>
    /// 获取 全局边界线
    /// </summary>
    /// <param name="rects"></param>
    /// <returns></returns>
    public static (List<double> xlines, List<double> ylines) GetGlobalLines(List<RectInfo> rects)
    {
        var xLines = new SortedSet<double>();
        var yLines = new SortedSet<double>();

        foreach (var r in rects)
        {
            xLines.Add(r.Rect.X);
            xLines.Add(r.Rect.X + r.Rect.Width);

            yLines.Add(r.Rect.Y);
            yLines.Add(r.Rect.Y + r.Rect.Height);
        }

        return (xLines.ToList(), yLines.ToList());
    }
    /// <summary>
    /// (1)收集所有叶子节点的矩形信息
    /// </summary>
    public static void CollectRects(
        LayoutNode node,
        RectD rect,
        List<RectInfo> result)
    {
        if (node is CellLayoutNode leaf)
        {
            result.Add(RectInfo.Content(leaf, rect));
            return;
        }

        var split = (SplitterNode)node;
        var splitterSize = split.SplitterSize;
        if (split.Direction == CutDirectionEnum.Horizontal)
        { 
            var totalStar = rect.Height - splitterSize; 
            var h1 = totalStar * split.Ratio;
            var h2 = totalStar - h1;  
            // 上内容
            CollectRects(
                split.LeftNode,
                new RectD(rect.X, rect.Y, rect.Width, h1),
                result);

            // Splitter
            result.Add(
                RectInfo.Splitter(
                    split,
                    new RectD(rect.X, rect.Y + h1, rect.Width, splitterSize)));

            // 下内容
            CollectRects(
                split.RightNode,
                new RectD(rect.X, rect.Y + h1 + splitterSize, rect.Width, h2),
                result);
        }
        else // Vertical
        { 
            var totalStar = rect.Width - splitterSize; 
            var w1 = totalStar * split.Ratio;
            var w2 = totalStar - w1; 
            // 左内容
            CollectRects(
                split.LeftNode,
                new RectD(rect.X, rect.Y, w1, rect.Height),
                result);

            // Splitter
            result.Add(
                RectInfo.Splitter(
                    split,
                    new RectD(rect.X + w1, rect.Y, splitterSize, rect.Height)));

            // 右内容
            CollectRects(
                split.RightNode,
                new RectD(rect.X + w1 + splitterSize, rect.Y, w2, rect.Height),
                result);
        }
    }

    public static bool IsSplitterX(double x0, double x1, List<RectInfo> rects)
    {
        var temp = rects.Any(r =>
            r.Kind == RectKind.Splitter &&
            Math.Abs(r.Rect.X - x0) < 1e-6 &&
            Math.Abs(r.Rect.X + r.Rect.Width - x1) < 1e-6);
        return temp;
    }

    public static bool IsSplitterY(double y0, double y1, List<RectInfo> rects) =>
        rects.Any(r =>
            r.Kind == RectKind.Splitter &&
            Math.Abs(r.Rect.Y - y0) < 1e-6 &&
            Math.Abs(r.Rect.Y + r.Rect.Height - y1) < 1e-6);
}