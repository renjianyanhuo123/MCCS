using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.EventParams
{
    public record AddNodeEventParam
    {
        /// <summary>
        /// 保存发布者引用(方便找到对应的位置)
        /// </summary>
        public required string Source { get; set; }
        /// <summary>
        /// 添加的节点
        /// </summary>
        public required BaseNode Node { get; init; }
        ///// <summary>
        ///// 待变更的添加节点的位置
        ///// </summary>
        //public required string ChangeNodeId { get; init; }

        ///// <summary>
        ///// 是否添加节点
        ///// </summary>
        //public required bool IsAdded { get; init; }
    }
}
