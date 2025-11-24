using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.ProjectManager
{
    [Table(Name = "project_DataRecord")]
    public class ProjectDataRecordModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 数据记录ID
        /// </summary> 
        [Column(IsNullable = false, StringLength = 30)]
        public string RecordId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long Timestamp { get; set; }

        [Column(IsIgnore = true)]
        public List<ProjectSignalItemModel> SignalItems { get; set; } = [];
    }
}
