namespace TopoMojo.Data
{
    public static class StringExtensions
    {
        public static bool HasValue(this string s)
        {
            return !System.String.IsNullOrWhiteSpace(s);
        }
    }
}