using MCCS.WorkflowSetting.Models.Nodes;

namespace MCCS.WorkflowSetting.Models.Edges
{
    public class EdgeModel
    {
        public BaseNode From { get; set; }
        public BaseNode To { get; set; }
    }
}
