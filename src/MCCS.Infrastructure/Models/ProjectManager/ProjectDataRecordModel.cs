using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.ProjectManager
{
    [Table(Name = "project_DataRecord")]
    public record ProjectDataRecordModel
    {
        /// <summary>
        /// 数据记录ID
        /// </summary>
        [Column(IsPrimary = true)]
        public string Id { get; private set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long Timestamp { get; init; }

        [Column(IsIgnore = true)]
        public List<ProjectSignalItemModel> SignalItems { get; init; }
    }
}
