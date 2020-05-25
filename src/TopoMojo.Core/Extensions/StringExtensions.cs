using System;
using System.Linq;

namespace TopoMojo.Extensions
{
    public static class StringExtensions
    {

        public static string ToSlug(this string target)
        {
            string result = "";

            bool duplicate = false;

            foreach (char c in target.ToCharArray())
            {
                if (char.IsLetterOrDigit(c))
                {
                    result += c;

                    duplicate = false;
                }
                else
                {
                    if (!duplicate)
                        result += '-';

                    duplicate = true;
                }
            }

            return result.ToLower();
        }

        public static int ToSeconds(this string ts)
        {
            if (ts == string.Empty)
                return 0;

            if (int.TryParse(ts.Substring(0, ts.Length - 1), out int value))
            {
                char type = ts.Trim().ToCharArray().Last();
                int factor = 1;

                switch (type)
                {
                    case 'y':
                    factor = 86400 * 365;
                    break;

                    case 'w':
                    factor = 86400 * 7;
                    break;

                    case 'd':
                    factor = 86400;
                    break;

                    case 'h':
                    factor = 3600;
                    break;

                    case 'm':
                    factor = 60;
                    break;
                }

                return value * factor;
            }

            throw new ArgumentException("invalid simple-timespan");
        }

        public static DateTime ToDatePast(this string ts)
        {
            return DateTime.UtcNow
                .Subtract(
                    new TimeSpan(
                        0,
                        0,
                        ts.ToSeconds()
                    )
                );
        }
    }
}
