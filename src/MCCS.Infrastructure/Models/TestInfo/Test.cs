using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.TestInfo
{
    [Table(Name = "tb_testinfo")]
    public class Test : BaseModel
    {
        /// <summary>
        /// 试样编号
        /// </summary>
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 试样名称
        /// </summary>
        [MaxLength(50)]
        public string? Name { get; set; } = null;

        /// <summary>
        /// 试样标准
        /// </summary>
        [MaxLength(100)]
        public string Standard
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 试验人员
        /// </summary>
        [MaxLength(50)]
        public string Person
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public string Remark
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 文件路径
        /// </summary>
        [MaxLength(100)]
        public string FilePath
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 试验状态
        /// </summary>
        public TestStatus Status
        {
            get;
            set;
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        [Column(IsNullable = true, MapType = typeof(string))]
        public DateTimeOffset? StartTime
        {
            get;
            set;
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        [Column(IsNullable = true, MapType = typeof(string))]
        public DateTimeOffset? EndTime
        {
            get;
            set;
        }
    }
}
