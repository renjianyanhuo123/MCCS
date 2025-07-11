using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.SystemManager
{
    [Table(Name = "tb_variableInfo")]
    public class VariableInfo : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public required string Name { get; set; } = string.Empty;
        /// <summary>
        /// 内部名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string VariableId { get; set; } = string.Empty;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsShowable { get; set; }
        /// <summary>
        /// 是否可控制
        /// </summary>
        public bool IsCanControl { get; set; }
        /// <summary>
        /// 是否可标定
        /// </summary>
        public bool IsCanCalibration { get; set; }

        /// <summary>
        /// 是否可设置极限
        /// </summary>
        public bool IsCanSetLimit { get; set; }
        /// <summary>
        /// 硬件信息
        /// ID,ID1,ID2
        /// </summary>
        [Column(IsNullable = true, StringLength = 100)]
        public string? HardwareInfos { get; set; }
    }
}
