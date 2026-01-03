namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 布尔参数
    /// </summary>
    public class BooleanParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.Boolean;
        public override Type ValueType => typeof(bool);

        /// <summary>
        /// 真值显示文本
        /// </summary>
        public string TrueText { get; set; } = "是";

        /// <summary>
        /// 假值显示文本
        /// </summary>
        public string FalseText { get; set; } = "否";

        public override IComponentParameter Clone()
        {
            var clone = new BooleanParameter();
            CloneBase(clone);
            clone.TrueText = TrueText;
            clone.FalseText = FalseText;
            return clone;
        }
    }
}
