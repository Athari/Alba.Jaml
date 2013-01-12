using System;
using System.Globalization;

namespace Alba.Jaml.MSInternal
{
    internal static class KS
    {
        public static bool EndsWith (string src, string target)
        {
            return src.EndsWith(target, StringComparison.Ordinal);
        }

        public static bool Eq (string a, string b)
        {
            return string.Equals(a, b, StringComparison.Ordinal);
        }

        public static string Fmt (string formatString, params object[] otherArgs)
        {
            return string.Format(CultureInfo.InvariantCulture, formatString, otherArgs);
        }

        public static int IndexOf (string src, string chars)
        {
            return src.IndexOf(chars, StringComparison.Ordinal);
        }

        public static bool StartsWith (string src, string target)
        {
            return src.StartsWith(target, StringComparison.Ordinal);
        }
    }
}