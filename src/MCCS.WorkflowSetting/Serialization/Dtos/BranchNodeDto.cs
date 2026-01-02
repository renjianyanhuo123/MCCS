using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 分支标题节点DTO
    /// </summary>
    public class BranchNodeDto : BaseNodeDto
    {
        /// <summary>
        /// 分支标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        public BranchNodeDto()
        {
            Type = NodeTypeEnum.Branch;
        }
    }
}
