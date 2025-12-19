using MCCS.UserControl.DynamicGrid.FlattenedGrid;

namespace MCCS.UserControl.DynamicGrid
{
    public record LayoutSettingModel
    {
        /// <summary>
        /// 所有的内容单元(包含Splitter)
        /// </summary>
        public required List<UiContentElement> Contents { get; init; }
        /// <summary>
        /// 空间结构
        /// </summary>
        public required LayoutNode SpatialStructure { get; set; } 
    }
}
