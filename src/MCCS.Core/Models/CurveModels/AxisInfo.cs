using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.CurveModels
{
    /// <summary>
    /// 轴信息种类
    /// </summary>
    public enum AxisTypeEnum : int
    {
        X,
        Y
    }

    /// <summary>
    /// 轴信息类
    /// </summary>
    [Table(Name = "tb_axisInfo")]
    public class AxisInfo : BaseModel
    {
        /// <summary>
        /// 轴的名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 100)]
        public required string AxisName { get; set; }
        /// <summary>
        /// 轴的种类
        /// </summary>
        public AxisTypeEnum AxisType { get; set; }
        /// <summary>
        /// 变量Id
        /// </summary>
        public long VariableId { get; set; }
        /// <summary>
        /// 曲线Id
        /// </summary>
        public long CurveId { get; set; }

        /// <summary>
        /// 最小时间或最小采样点
        /// </summary>
        public double MinLimit { get; set; }
        /// <summary>
        /// 最大时间或最大采样点
        /// </summary>
        public double MaxLimit { get; set; } = double.MaxValue;
        /// <summary>
        /// 坐标刻度间隔（可选）
        /// </summary>
        public double Unit { get; set; } = 0.01;
        /// <summary>
        /// 是否自动缩放
        /// </summary>
        public bool IsAutoScale { get; set; }
    }
}
