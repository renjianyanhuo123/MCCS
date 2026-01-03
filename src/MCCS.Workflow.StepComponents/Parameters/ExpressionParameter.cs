namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 表达式参数（支持变量引用）
    /// </summary>
    public class ExpressionParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.Expression;
        public override Type ValueType => typeof(string);

        /// <summary>
        /// 期望的返回类型
        /// </summary>
        public Type? ExpectedReturnType { get; set; }

        /// <summary>
        /// 可用的变量列表（用于自动完成提示）
        /// </summary>
        public List<ExpressionVariable> AvailableVariables { get; set; } = new();

        public override IComponentParameter Clone()
        {
            var clone = new ExpressionParameter();
            CloneBase(clone);
            clone.ExpectedReturnType = ExpectedReturnType;
            clone.AvailableVariables = AvailableVariables.Select(v => new ExpressionVariable
            {
                Name = v.Name,
                Type = v.Type,
                Description = v.Description
            }).ToList();
            return clone;
        }
    }

    /// <summary>
    /// 表达式变量
    /// </summary>
    public class ExpressionVariable
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 变量类型
        /// </summary>
        public Type? Type { get; set; }

        /// <summary>
        /// 变量描述
        /// </summary>
        public string? Description { get; set; }
    }
}
