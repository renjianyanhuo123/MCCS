using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 决策节点DTO
    /// </summary>
    public class DecisionNodeDto : BaseNodeDto
    {
        /// <summary>
        /// 是否折叠状态
        /// </summary>
        public bool IsCollapse { get; set; } = true;

        /// <summary>
        /// 子分支列表
        /// </summary>
        public List<BranchStepListDto> Branches { get; set; } = [];

        public DecisionNodeDto()
        {
            Type = NodeTypeEnum.Decision;
        }
    }
}
