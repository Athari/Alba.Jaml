using System.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private static string FormatScalarPropertyValue (string value)
        {
            return MarkupAliases.Aggregate(value, (v, alias) => v.Replace(alias[0], alias[1]));
        }
    }
}