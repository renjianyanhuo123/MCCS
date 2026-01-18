using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.ProjectManager
{
    [Table(Name = "project_DataRecordItem")]
    public class ProjectDataRecordItemModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(IsNullable = false, StringLength = 30)]
        public required string RecordId { get; set; }

        [Column(IsNullable = false, StringLength = 30)]
        public required string PseudoChannelKey { get; set; }

        public required long PseudoChannelId { get; set; }

        public required float Value { get; set; }
        [Column(IsNullable = false, StringLength = 20)]
        public required string Unit { get; set; }
    }
}
