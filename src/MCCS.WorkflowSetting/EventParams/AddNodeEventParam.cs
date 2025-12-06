using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.EventParams
{
    public record AddNodeEventParam
    {
        /// <summary>
        /// 保存发布者引用
        /// </summary>
        public required string Source { get; set; }
        public required BaseNode Node { get; init; }
    }
}
