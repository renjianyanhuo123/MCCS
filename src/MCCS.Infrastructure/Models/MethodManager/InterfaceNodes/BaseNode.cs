namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{
    public abstract class BaseNode
    {
        /// <summary>
        /// 单元格唯一标识符
        /// </summary>
        public string Id { get; private set; } = "";

        ///// <summary>
        ///// 父节点
        ///// </summary>
        public BaseNode? Parent { get;private set; }

        /// <summary>
        /// 左子节点
        /// </summary>
        public BaseNode? LeftNode { get; set; }

        /// <summary>
        /// 右子节点
        /// </summary>
        public BaseNode? RightNode { get; set; }
    }
}
