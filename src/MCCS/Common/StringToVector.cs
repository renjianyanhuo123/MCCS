namespace MCCS.Common
{
    public static class StringToVector
    {
        public static T ToVector<T>(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return default!;
            }
            var parts = str.Split(',');
            if (parts.Length < 3)
            {
                throw new ArgumentException("String must contain exactly three comma-separated values.");
            }
            try
            {
                var x = Convert.ToSingle(parts[0]);
                var y = Convert.ToSingle(parts[1]);
                var z = Convert.ToSingle(parts[2]);
                if (parts.Length <= 3)
                    return (T)(Activator.CreateInstance(typeof(T), x, y, z) ?? throw new InvalidOperationException());
                var w = parts.Length > 3 ? Convert.ToSingle(parts[3]) : 0.0;
                return (T)(Activator.CreateInstance(typeof(T), x, y, z, w) ?? throw new InvalidOperationException());
            }
            catch (Exception ex)
            {
                throw new FormatException("Failed to convert string to vector.", ex);
            }
        }
    }
}
