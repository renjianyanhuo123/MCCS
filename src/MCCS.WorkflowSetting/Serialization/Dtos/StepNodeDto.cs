using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 步骤节点DTO
    /// </summary>
    public class StepNodeDto : BaseNodeDto
    {
        /// <summary>
        /// 节点标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 标题背景色（存储为十六进制颜色字符串）
        /// </summary>
        public string TitleBackgroundColor { get; set; } = "#FF9955";

        public StepNodeDto()
        {
            Type = NodeTypeEnum.Process;
        }
    }
}
