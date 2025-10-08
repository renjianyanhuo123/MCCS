using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class AddOpNode : BaseNode
    {
        public AddOpNode(string name, NodeTypeEnum type, double width, double height, Action<AddOpEventArgs> handle, int level = 0, int order = -1) : base(name, type, width, height, level, order)
        {
            Content.MouseLeftButtonDown += (sender, e) =>
            {
                handle?.Invoke(new AddOpEventArgs { NodeId = Id});
            };
        }
         
    }
}
