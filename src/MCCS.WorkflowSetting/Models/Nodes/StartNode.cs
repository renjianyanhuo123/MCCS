namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StartNode : BaseNode
    {
        /*
         *Width = 60,
           Height = 60,
         */

        public StartNode(BaseNode? parent = null)
        {
            Width = 60;
            Height = 60;
            Parent = parent;
            Type = NodeTypeEnum.Start;
            Name = "Start";
        }
    }
}
