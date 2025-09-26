using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.MethodManager
{
    [Table(Name = "method_main")]
    public class MethodModel : BaseModel
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 100)]
        public required string Name { get; set; }
        /// <summary>
        /// 方法类型
        /// </summary>
        public MethodTypeEnum MethodType { get; set; }
        /// <summary>
        /// 试验类型
        /// </summary>
        public TestTypeEnum TestType { get; set; }
        /// <summary>
        /// 方法编号
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string Code { get; set; }
        /// <summary>
        /// 方法标准
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string Standard { get; set; } 
        /// <summary>
        /// 文件路径
        /// </summary>
        [Column(IsNullable = false, StringLength = 500)]
        public required string FilePath { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(IsNullable = true, StringLength = -2)]
        public string? Remark { get; set; }
    }
}
