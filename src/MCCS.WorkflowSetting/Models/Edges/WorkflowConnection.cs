using System.Windows;
using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Models.Edges
{
    public class WorkflowConnection
    {
        public string Id { get; set; }
        public BaseNode SourceNode { get; set; }
        public BaseNode TargetNode { get; set; }
        public ConnectionTypeEnum Type { get; set; }
        public string Label { get; set; } // 分支条件标签

        // 连接点位置（相对于节点的位置）
        public Point SourcePoint { get; set; }
        public Point TargetPoint { get; set; }
    }
}
