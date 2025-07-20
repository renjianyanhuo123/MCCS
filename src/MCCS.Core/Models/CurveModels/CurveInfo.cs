using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.CurveModels
{
    /// <summary>
    /// 曲线信息
    /// </summary>
    [Table(Name = "tb_curveInfo")]
    public class CurveInfo : BaseModel
    {
        /// <summary>
        /// 曲线ID
        /// </summary>
        [Column(IsNullable = false, StringLength = 100)]
        public required string CurveId { get; set; }
        /// <summary>
        /// 曲线名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 100)]
        public required string CurveName { get; set; }
        /// <summary>
        /// 默认的数据采集频率
        /// 默认1次/S
        /// </summary>
        public double DefaultFrequency { get; set; } = 1.0;
        /// <summary>
        /// 曲线类型
        /// </summary>
        public CurveTypeEnum CurveType { get; set; } 
        
    }
}
