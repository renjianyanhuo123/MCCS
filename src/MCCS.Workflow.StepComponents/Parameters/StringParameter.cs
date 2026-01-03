using System.Text.RegularExpressions;

namespace MCCS.Workflow.StepComponents.Parameters
{
    /// <summary>
    /// 字符串参数
    /// </summary>
    public class StringParameter : BaseComponentParameter
    {
        public override ParameterType ParameterType => ParameterType.String;
        public override Type ValueType => typeof(string);

        /// <summary>
        /// 最大长度
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// 最小长度
        /// </summary>
        public int? MinLength { get; set; }

        public override ParameterValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.IsValid) return baseResult;

            if (Value is string strValue)
            {
                if (MinLength.HasValue && strValue.Length < MinLength.Value)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 长度不能小于 {MinLength.Value}");
                }

                if (MaxLength.HasValue && strValue.Length > MaxLength.Value)
                {
                    return ParameterValidationResult.Invalid($"{DisplayName} 长度不能大于 {MaxLength.Value}");
                }

                if (!string.IsNullOrEmpty(ValidationPattern))
                {
                    if (!Regex.IsMatch(strValue, ValidationPattern))
                    {
                        return ParameterValidationResult.Invalid(ValidationMessage ?? $"{DisplayName} 格式不正确");
                    }
                }
            }

            return ParameterValidationResult.Valid();
        }

        public override IComponentParameter Clone()
        {
            var clone = new StringParameter();
            CloneBase(clone);
            clone.MaxLength = MaxLength;
            clone.MinLength = MinLength;
            return clone;
        }
    }

    /// <summary>
    /// 多行文本参数
    /// </summary>
    public class MultilineTextParameter : StringParameter
    {
        public override ParameterType ParameterType => ParameterType.MultilineText;

        /// <summary>
        /// 显示行数
        /// </summary>
        public int Rows { get; set; } = 5;

        public override IComponentParameter Clone()
        {
            var clone = new MultilineTextParameter();
            CloneBase(clone);
            clone.MaxLength = MaxLength;
            clone.MinLength = MinLength;
            clone.Rows = Rows;
            return clone;
        }
    }

    /// <summary>
    /// 密码参数
    /// </summary>
    public class PasswordParameter : StringParameter
    {
        public override ParameterType ParameterType => ParameterType.Password;

        public override IComponentParameter Clone()
        {
            var clone = new PasswordParameter();
            CloneBase(clone);
            clone.MaxLength = MaxLength;
            clone.MinLength = MinLength;
            return clone;
        }
    }
}
