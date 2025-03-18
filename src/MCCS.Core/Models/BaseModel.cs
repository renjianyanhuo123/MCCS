using FreeSql.DataAnnotations;

namespace MCCS.Core.Models
{
    public class BaseModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set;}
        [Column(MapType = typeof(string))]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
        [Column(MapType = typeof(string))]
        public DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}
