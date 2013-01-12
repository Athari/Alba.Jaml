using System.Globalization;
using System.Resources;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
// ReSharper disable MethodOverloadWithOptionalParameter
namespace Alba.Jaml.MSInternal
{
    internal static class SR
    {
        private static ResourceManager _resourceManager = new ResourceManager("Alba.Jaml.MSInternal.ExceptionStringTable", typeof(SR).Assembly);

        internal static string Get (string id)
        {
            return _resourceManager.GetString(id) ?? "Unavailable";
        }

        internal static string Get (string id, params object[] args)
        {
            string format = _resourceManager.GetString(id) ?? "Unavailable";
            return args != null && args.Length > 0 ? string.Format(CultureInfo.CurrentCulture, format, args) : format;
        }
    }
}