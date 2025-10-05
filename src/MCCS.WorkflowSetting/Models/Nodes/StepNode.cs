namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepNode(string name, NodeTypeEnum type, double width, double height, int level = 0, int order = -1)
        : BaseNode(name, type, width, height, level, order);
}
