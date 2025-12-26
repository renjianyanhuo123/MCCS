namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{
    public class CellNode : BaseNode 
    {

        public CellNode()
        {
        }

        public CellNode(
            string id,
            NodeTypeEnum type,
            long nodeId,
            string? paramterJson,
            string? parentId = null,
            string? leftNodeId = null,
            string? rightNodeId = null)
            : base(id, type, parentId, leftNodeId, rightNodeId)
        {
            NodeId = nodeId;
            ParamterJson = paramterJson;
        }

        /// <summary>
        /// 配置的UIComponent节点Id
        /// </summary>
        public long NodeId { get; set; } 

        /// <summary>
        /// 所有的参数
        /// </summary>
        public string? ParamterJson{ get; set;}
    }
}
