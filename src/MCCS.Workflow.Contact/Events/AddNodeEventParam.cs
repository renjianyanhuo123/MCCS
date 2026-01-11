using MCCS.Workflow.Contact.Models;

namespace MCCS.Workflow.Contact.Events
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
        public required NodeInfo? Node { get; init; }
    }
}
