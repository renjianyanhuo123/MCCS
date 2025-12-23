using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.MethodManager
{
    [Table(Name = "method_interfaceComponents")]
    public class MethodUiComponentsModel : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public string Title { get; set; } = string.Empty;

        [Column(IsNullable = true, StringLength = 200)]
        public string? Icon { get; set; }

        [Column(IsNullable = false, StringLength = 200)]
        public required string ViewTypeName { get; set; }

        [Column(IsNullable = true, StringLength = -2)]
        public string? ParametersJson { get; set; }
    }
}
