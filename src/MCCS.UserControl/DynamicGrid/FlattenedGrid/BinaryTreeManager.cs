using System.Windows;

namespace MCCS.UserControl.DynamicGrid.FlattenedGrid
{
    /// <summary>
    /// 二叉树维护
    /// </summary>
    public sealed class BinaryTreeManager(LayoutNode root)
    {
        private double _defaultRowAddLength = 0.5;
        private double _defaultColumnAddLength = 0.5;
        private const double _unitAddLength = 1e-3;
        private List<RectInfo>? _rectInfos;

        /// <summary>
        /// 获取行列定义
        /// </summary>
        /// <returns></returns>
        public (List<(GridUnitType, double)> rowDifitions, List<(GridUnitType, double)> columnDifitions) GetRowAndColumnDifitions()
        { 
            if (_rectInfos == null)
            {
                // 获取所有的矩形信息
                _rectInfos = [];
                FlattenOperation.CollectRects(root, new RectD(0, 0, 1, 1), _rectInfos);
            } 
            var columnDefinitions = new List<(GridUnitType, double)>();
            var rowDefinitions = new List<(GridUnitType, double)>();
            (List<double> xList, List<double> yList) = FlattenOperation.GetGlobalLines(_rectInfos);
            for (var i = 0; i < xList.Count - 1; i++)
            {
                var x0 = xList[i];
                var x1 = xList[i + 1];
                columnDefinitions.Add(Math.Abs(Math.Abs(x1 - x0) - FlattenOperation._splitterThickness) < 1e-6
                    ? (GridUnitType.Pixel, 5) 
                    : (GridUnitType.Star, x1 - x0));
            }

            for (var i = 0; i < yList.Count - 1; i++)
            {
                var y0 = yList[i];
                var y1 = yList[i + 1];
                rowDefinitions.Add(Math.Abs(y1 - y0) - FlattenOperation._splitterThickness < 1e-6
                    ? (GridUnitType.Pixel, 5)
                    : (GridUnitType.Star, y1 - y0));
            }
            return (rowDefinitions, columnDefinitions);
        }

        /// <summary>
        /// 获取元素在网格中的位置
        /// </summary>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        public Dictionary<string, (int row, int col, int rSpan, int cSpan)> GetElementDisplacement(LayoutNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            if (_rectInfos == null)
            {
                _rectInfos = [];
                // 获取所有的矩形信息
                FlattenOperation.CollectRects(rootNode, new RectD(0, 0, 1, 1), _rectInfos);
            } 
            var placements = new Dictionary<string, (int r, int c, int rs, int cs)>();
            (List<double> xList, List<double> yList) = FlattenOperation.GetGlobalLines(_rectInfos);
            foreach (var info in _rectInfos)
            {
                var r0 = FlattenOperation.IndexOf(yList, info.Rect.Y);
                var r1 = FlattenOperation.IndexOf(yList, info.Rect.Y + info.Rect.Height);

                var c0 = FlattenOperation.IndexOf(xList, info.Rect.X);
                var c1 = FlattenOperation.IndexOf(xList, info.Rect.X + info.Rect.Width);

                var rowSpan = r1 - r0;
                var colSpan = c1 - c0;
                if (info.Leaf != null)
                {
                    placements[info.Leaf.Id] = (r0, c0, rowSpan, colSpan);
                }

                if (info.Split != null)
                {
                    placements[info.Split.Id] = (r0, c0, rowSpan, colSpan);
                }
            }
            return placements;
        }
        /// <summary>
        /// 横切
        /// </summary>
        /// <param name="targetNodeId">操作的节点Id</param>
        /// <param name="contentNodeId">新增的内容节点Id</param>
        /// <param name="splitterNodeId">横切节点Id</param>
        public void CutHorizontal(string targetNodeId, string contentNodeId, string splitterNodeId)
        {
            if (_rectInfos == null)
            {
                // 获取所有的矩形信息
                _rectInfos = [];
                FlattenOperation.CollectRects(root, new RectD(0, 0, 1, 1), _rectInfos);
            } 
            var targetNode = _rectInfos.FirstOrDefault(c => c.Leaf != null && c.Leaf.Id == targetNodeId)?.Leaf;
            if (targetNode is not { Parent: SplitterNode parentSplitterNode }) return;
            var newLeafNode = new CellLayoutNode(contentNodeId);
            var spliiterNode = new SplitterNode(splitterNodeId, _defaultRowAddLength, CutDirectionEnum.Horizontal, targetNode, newLeafNode)
            {
                Parent = targetNode.Parent
            }; 
            if (parentSplitterNode.LeftNode == targetNode)
            {
                parentSplitterNode.LeftNode = spliiterNode;
            }
            else
            {
                parentSplitterNode.RightNode = spliiterNode;
            }
            _defaultRowAddLength += _unitAddLength;
            // 恢复未刷新状态
            _rectInfos = null;
        }

        /// <summary>
        /// 竖切
        /// </summary>
        /// <param name="targetNodeId">操作的节点Id</param>
        /// <param name="contentNodeId">新增的内容节点Id</param>
        /// <param name="splitterNodeId">竖切节点Id</param>
        public void CutVertical(string targetNodeId, string contentNodeId, string splitterNodeId)
        {
            if (_rectInfos == null)
            {
                // 获取所有的矩形信息
                _rectInfos = [];
                FlattenOperation.CollectRects(root, new RectD(0, 0, 1, 1), _rectInfos);
            }
            var targetNode = _rectInfos.FirstOrDefault(c => c.Leaf != null && c.Leaf.Id == targetNodeId)?.Leaf;
            if (targetNode is not { Parent: SplitterNode parentSplitterNode }) return;
            var newLeafNode = new CellLayoutNode(contentNodeId);
            var spliiterNode = new SplitterNode(splitterNodeId, _defaultColumnAddLength, CutDirectionEnum.Vertical, targetNode, newLeafNode)
            {
                Parent = targetNode.Parent
            };
            if (parentSplitterNode.LeftNode == targetNode)
            {
                parentSplitterNode.LeftNode = spliiterNode;
            }
            else
            {
                parentSplitterNode.RightNode = spliiterNode;
            }
            _defaultColumnAddLength += _unitAddLength;
            // 恢复未刷新状态
            _rectInfos = null;
        } 
    }
}
