namespace MCCS.WorkflowSetting.Models.Nodes
{
    public enum NodeTypeEnum
    {
        Start,      // 开始节点
        End,        // 结束节点
        Process,    // 普通处理节点
        Decision,   // 判断节点（分支）
        Action,     // 添加操作节点
        Branch,     // 分支节点
        StepList,   // 主流程
        BranchStepList // 分支子流程
    }
}
