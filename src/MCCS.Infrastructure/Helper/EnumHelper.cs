using System.ComponentModel;
using System.Reflection;

namespace MCCS.Infrastructure.Helper
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            DescriptionAttribute? attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
