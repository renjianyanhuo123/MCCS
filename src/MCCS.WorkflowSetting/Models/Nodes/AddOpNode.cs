namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class AddOpNode : BaseNode
    {
        public AddOpNode( )  
        {
            //Content.MouseLeftButtonDown += (sender, e) =>
            //{
            //    handle?.Invoke(new AddOpEventArgs { NodeId = Id});
            //};
            Type = NodeTypeEnum.Action;
        }
         
    }
}
