using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.MethodManager
{
    [Table(Name = "method_workflowSetting")]
    public class MethodWorkflowSettingModel : BaseModel
    {
        /// <summary>
        /// 方法ID
        /// </summary>
        public long MethodId { get; set; }

        /// <summary>
        /// 流程节点Json配置
        /// </summary>
        [Column(IsNullable = false, StringLength = -2)]
        public required string WorkflowSetting { get; set; }
    }
}
