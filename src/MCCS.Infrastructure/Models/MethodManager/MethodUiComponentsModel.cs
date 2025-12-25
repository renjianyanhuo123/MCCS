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
        [Column(IsNullable = true, StringLength = 500)]
        public string? Description { get; set; }
        
        [Column(IsNullable = true, StringLength = 100)]
        public string? SetParamViewName { get; set; }

        /// <summary>
        /// 是否可以设置参数
        /// </summary>
        public bool IsCanSetParam { get; set; }

        /// <summary>
        /// 组件类型
        /// </summary>
        public UiComponentTypeEnum ComponentType { get; set; }
    }
}
