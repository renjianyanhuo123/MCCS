namespace MCCS.UserControl.DynamicGrid.FlattenedGrid
{
    /// <summary>
    /// 抽象的节点视图模型
    /// </summary>
    public abstract class LayoutNode
    { 
        /// <summary>
        /// 单元格唯一标识符
        /// </summary>
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N");
        ///// <summary>
        ///// 父节点
        ///// </summary>
        public LayoutNode? Parent { get; set; } 
        ///// <summary>
        ///// 每个格子一个独立内容
        ///// </summary>
        // public required FrameworkElement Content { get; set; } 

        // public CellTypeEnum CellType { get; set; } = CellTypeEnum.EditableElement;
    }
}
