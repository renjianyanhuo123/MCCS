namespace MCCS.Models.CurveModels
{
    public class XYBindCollectionItem
    {
        /// <summary>
        /// 用于存储对外获取数据的ID
        /// </summary>
        public long Id { get; init; }
        /// <summary>
        /// 展示名称
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// 展示的中文名称
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; init; } = string.Empty;
    }
}
