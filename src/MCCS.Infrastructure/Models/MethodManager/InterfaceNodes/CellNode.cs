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
            string nodeId,
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
        /// 在组件的特性中定义
        /// </summary>
        public string NodeId { get; set; } 

        /// <summary>
        /// 所有的参数
        /// </summary>
        public string? ParamterJson{ get; set;} 
    }
}
