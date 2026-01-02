using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 分支步骤列表DTO
    /// </summary>
    public class BranchStepListDto : BaseNodeDto
    {
        /// <summary>
        /// 分支中的所有节点
        /// </summary>
        public List<BaseNodeDto> Nodes { get; set; } = [];

        public BranchStepListDto()
        {
            Type = NodeTypeEnum.BranchStepList;
        }
    }
}
