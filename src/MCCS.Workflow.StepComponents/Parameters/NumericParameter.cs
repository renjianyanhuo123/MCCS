namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 整数参数
    /// </summary>
    public class IntegerParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.Integer;
        public override Type ValueType => typeof(int);

        /// <summary>
        /// 步进值
        /// </summary>
        public int Step { get; set; } = 1;

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value != null)
            {
                if (!int.TryParse(Value.ToString(), out int intValue))
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 必须是整数");
                }

                if (MinValue is int min && intValue < min)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 不能小于 {min}");
                }

                if (MaxValue is int max && intValue > max)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 不能大于 {max}");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new IntegerParameter();
            CloneBase(clone);
            clone.Step = Step;
            return clone;
        }
    }

    /// <summary>
    /// 浮点数参数
    /// </summary>
    public class DoubleParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.Double;
        public override Type ValueType => typeof(double);

        /// <summary>
        /// 小数位数
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// 步进值
        /// </summary>
        public double Step { get; set; } = 0.1;

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value != null)
            {
                if (!double.TryParse(Value.ToString(), out double doubleValue))
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 必须是数字");
                }

                if (MinValue is double min && doubleValue < min)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 不能小于 {min}");
                }

                if (MaxValue is double max && doubleValue > max)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 不能大于 {max}");
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new DoubleParameter();
            CloneBase(clone);
            clone.DecimalPlaces = DecimalPlaces;
            clone.Step = Step;
            return clone;
        }
    }
}
