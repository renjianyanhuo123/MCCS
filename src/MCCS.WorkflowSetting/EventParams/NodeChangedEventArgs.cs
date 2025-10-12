using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.EventParams
{
    /// <summary>
    /// 节点变更事件参数
    /// </summary>
    public class NodeChangedEventArgs(BaseNode sourceNode, string changeType, object? changeData = null)
        : EventArgs
    {
        public BaseNode SourceNode { get; } = sourceNode;
        public string ChangeType { get; } = changeType;
        public object? ChangeData { get; } = changeData;
        public bool Handled { get; set; } = false;
    }
}
