namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class TempPlaceholderAddNode : BaseNode
    {
        public TempPlaceholderAddNode(BaseNode parent)
        {
            Width = 240; 
            Height = 100;
            Parent = this;
            Type = NodeTypeEnum.TempPlaceholder;
        }
    }
}
