using System.Globalization;
using System.Resources;

// TODO Extract real resources somehow
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
// ReSharper disable MethodOverloadWithOptionalParameter
namespace Alba.Jaml.MSInternal
{
    internal static class SR
    {
        private static ResourceManager _resourceManager = new ResourceManager("ExceptionStringTable", typeof(SR).Assembly);

        internal static string Get (string id)
        {
            return id;
//            string str = _resourceManager.GetString(id);
//            if (str == null) {
//                str = _resourceManager.GetString("Unavailable");
//            }
//            return str;
        }

        internal static string Get (string id, params object[] args)
        {
            return id + " (" + string.Join(", ", args) + ")";
//            string format = _resourceManager.GetString(id);
//            if (format == null) {
//                return _resourceManager.GetString("Unavailable");
//            }
//            if ((args != null) && (args.Length > 0)) {
//                format = string.Format(CultureInfo.CurrentCulture, format, args);
//            }
//            return format;
        }

        internal static ResourceManager ResourceManager
        {
            get { return _resourceManager; }
        }
    }
}