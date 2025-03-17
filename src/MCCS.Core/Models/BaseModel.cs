using FreeSql.DataAnnotations;

namespace MCCS.Core.Models
{
    public class BaseModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set;}
        public DateTimeOffset CreateTime { get; set; }

        public DateTimeOffset UpdateTime { get; set; } 

        public bool IsDeleted { get; set; } = false;
    }
}
