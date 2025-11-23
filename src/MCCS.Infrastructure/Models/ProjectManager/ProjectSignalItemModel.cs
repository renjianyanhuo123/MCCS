using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.ProjectManager
{
    [Table(Name = "project_SignalItem")]
    public record ProjectSignalItemModel
    {
        [Column(IsPrimary = true)]
        public long RecordId { get; set; }

        [Column(IsPrimary = true)]
        public required string SignalKey { get; init; }

        public required float Value { get; init; }

        public required string Unit { get; init; }
    }
}
