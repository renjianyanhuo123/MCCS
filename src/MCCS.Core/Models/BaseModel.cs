using FreeSql.DataAnnotations;

namespace MCCS.Core.Models
{
    public class BaseModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set;}
        [Column(MapType = typeof(string))]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.Now;
        [Column(MapType = typeof(string))]
        public DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.Now;

        public bool IsDeleted { get; set; } = false;
    }
}
