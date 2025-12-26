namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{ 
    public class BaseNode
    {
        public BaseNode()
        {
        }

        public BaseNode(
            string id,
            NodeTypeEnum nodeType,
            string? parentId = null,
            string? leftNodeId = null,
            string? rightNodeId = null)
        {
            Id = id;
            NodeType = nodeType;
            ParentId = parentId;
            LeftNodeId = leftNodeId;
            RightNodeId = rightNodeId;
        }

        /// <summary>
        /// 单元格唯一标识符
        /// </summary>
        public string Id { get; set; }

        public NodeTypeEnum NodeType { get; set; } 

        ///// <summary>
        ///// 父节点
        ///// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 左子节点
        /// </summary>
        public string? LeftNodeId { get; set; }

        /// <summary>
        /// 右子节点
        /// </summary>
        public string? RightNodeId { get; set; }
    }
}
