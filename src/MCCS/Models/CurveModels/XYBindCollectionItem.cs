namespace MCCS.Models.CurveModels
{
    /// <summary>
    /// 用于序列化
    /// </summary>
    public class XyBindCollectionItem
    {
        /// <summary>
        /// 用于存储对外获取数据的ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 展示名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 展示的中文名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;
    }
}
