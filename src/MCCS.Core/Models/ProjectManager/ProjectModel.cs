using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.ProjectManager
{
    [Table(Name = "project_main")]
    public class ProjectModel : BaseModel
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 100)]
        public required string Name { get; set; }
        /// <summary>
        /// 方法ID
        /// </summary>
        public long MethodId { get; set; }
        [Column(IsNullable = false, StringLength = 100)]
        public required string MethodName { get; set; }
        /// <summary>
        /// 项目编号
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string Code { get; set; }
        /// <summary>
        /// 项目标准
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public required string Standard { get; set; }
        /// <summary>
        /// 试验人员
        /// </summary>
        [Column(IsNullable = true, StringLength = 100)]
        public string? Person { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        [Column(IsNullable = false, StringLength = -2)]
        public required string FilePath { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(IsNullable = true, StringLength = -2)]
        public string? Remark { get; set; }
    }
}
