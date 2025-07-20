namespace MCCS.Core.Domain.Curves
{
    public record AxisEntity
    {
        public required string AxisName { get; init; }
        /// <summary>
        /// 变量Id
        /// </summary>
        public long VariableId { get; init; }
        /// <summary>
        /// 曲线Id
        /// </summary>
        public long CurveId { get; init; }

        /// <summary>
        /// 最小时间或最小采样点
        /// </summary>
        public double MinLimit { get; init; }
        /// <summary>
        /// 最大时间或最大采样点
        /// </summary>
        public double MaxLimit { get; init; }
        /// <summary>
        /// 坐标刻度间隔（可选）
        /// </summary>
        public double Unit { get; init; }
        /// <summary>
        /// 是否自动缩放
        /// </summary>
        public bool IsAutoScale { get; init; }
    }
}
