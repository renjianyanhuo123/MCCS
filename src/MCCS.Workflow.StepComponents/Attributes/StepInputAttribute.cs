namespace MCCS.Workflow.StepComponents.Attributes
{
    /// <summary>
    /// 标记步骤输入属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class StepInputAttribute : Attribute
    {
        /// <summary>
        /// 参数名称（用于映射）
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }

        public StepInputAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 标记步骤输出属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class StepOutputAttribute : Attribute
    {
        /// <summary>
        /// 输出名称（用于映射）
        /// </summary>
        public string Name { get; }

        public StepOutputAttribute(string name)
        {
            Name = name;
        }
    }
}
