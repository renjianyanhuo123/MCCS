using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace MCCS.Core.Models.SystemSetting
{
    [Table(Name = "tb_systemmenu")]
    public class SystemMenu : BaseModel
    {
        [MaxLength(50)] 
        public string Key { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Icon { get; set; } = string.Empty;
        public MenuType Type { get; set; }
        public bool IsEnabled { get; set; } = true;
        public long ParentId { get; set; } = 0;
    }
}
