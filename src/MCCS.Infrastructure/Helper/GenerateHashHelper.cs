namespace MCCS.Infrastructure.Helper
{
    public static class HighPerformanceRandomHash
    {
        private static readonly Random Random = new();

        public static string GenerateRandomHash6()
        {
            Span<char> hash = stackalloc char[6];
            ReadOnlySpan<char> hexChars = "0123456789abcdef";

            for (var i = 0; i < 6; i++)
            {
                hash[i] = hexChars[Random.Next(16)];
            }

            return new string(hash);
        }
    }
}
