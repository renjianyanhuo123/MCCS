namespace MCCS.Components.LayoutRootComponents
{
    public class CellViewModel : BindableBase
    {
        /// <summary>
        /// 单元格唯一标识符
        /// </summary>
        public string Id { get; private set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 行号，从0开始
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// 列号，从0开始
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 行跨度，默认1
        /// </summary>
        public int RowSpan { get; set; } = 1;

        /// <summary>
        /// 列跨度，默认1
        /// </summary>
        public int ColumnSpan { get; set; } = 1;

        /// <summary>
        /// 每个格子一个独立内容
        /// </summary>
        public object Content { get; set; }
    }
}
