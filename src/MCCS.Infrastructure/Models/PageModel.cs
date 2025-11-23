namespace MCCS.Infrastructure.Models
{
    public class PageModel<T> where T : BaseModel
    {
        public List<T> Items { get; set; } = [];
        public long TotalCount { get; set; } 
    }
}
