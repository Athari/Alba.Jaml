using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private const string ReIdent = @"[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}][\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        private static readonly Regex ReBindingElementName = new Regex(
            @"^\{= ?ref\.(?<ElementName>" + ReIdent + @")(?:\.(?<Path>[^,\}]+))?",
            RegexOptions.Singleline);
        private static readonly Regex ReBindingSelf = new Regex(
            @"^\{= ?this(?:\.(?<Path>[^,\}]+))?",
            RegexOptions.Singleline);
        private static readonly Regex ReBindingTemplatedParent = new Regex(
            @"^\{= ?tpl(?:\.(?<Path>[^,\}]+))?",
            RegexOptions.Singleline);
        private static readonly Regex ReBindingAncestorType = new Regex(
            @"^\{= ?~(?<AncestorType>[^,\.\}]+)(?:\.(?<Path>[^,\}]+))?",
            RegexOptions.Singleline);
        private static readonly Regex ReBindingSource = new Regex(
            @"^\{=\ ?@(?<Source>\{
              (?:
                [^\{\}] | (?<paren>\{) | (?<-paren>\})
              )+
              (?(paren)(?!))
            \})(?:\.(?<Path>[^,\}]+))?",
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        private static object GetXAttrScalarProperty (JProperty prop)
        {
            // attribute="scalarValue"
            string value = prop.Value.ToString();
            if (!value.StartsWith("{="))
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(value));
            else
                return GetXBindingPropertyValue(prop);
        }

        private static string FormatScalarPropertyValue (string value)
        {
            return MarkupAliases.Aggregate(value, (v, alias) => v.Replace(alias[0], alias[1]));
        }

        private static object GetXBindingPropertyValue (JProperty prop)
        {
            string value = prop.Value.ToString();
            string scalarValue = FormatSimpleBindingScalarValue(value);
            if (scalarValue != null)
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(scalarValue));
            // TODO Format complex binding value (generate converters)
            return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(value));
        }

        private static string FormatSimpleBindingScalarValue (string value)
        {
            string scalarValue = null;
            Match match = ReBindingElementName.Match(value);
            if (match.Success) {
                scalarValue = string.IsNullOrEmpty(match.Groups["Path"].Value)
                    ? match.Result("{Binding ElementName=${ElementName}$'")
                    : match.Result("{Binding ${Path}, ElementName=${ElementName}$'");
            }
            if (scalarValue == null) {
                match = ReBindingSelf.Match(value);
                if (match.Success) {
                    scalarValue = string.IsNullOrEmpty(match.Groups["Path"].Value)
                        ? match.Result("{Binding RelativeSource={RelativeSource Self}$'")
                        : match.Result("{Binding ${Path}, RelativeSource={RelativeSource Self}$'");
                }
            }
            if (scalarValue == null) {
                match = ReBindingTemplatedParent.Match(value);
                if (match.Success) {
                    scalarValue = string.IsNullOrEmpty(match.Groups["Path"].Value)
                        ? match.Result("{Binding RelativeSource={RelativeSource TemplatedParent}$'")
                        : match.Result("{Binding ${Path}, RelativeSource={RelativeSource TemplatedParent}$'");
                }
            }
            if (scalarValue == null) {
                match = ReBindingAncestorType.Match(value);
                if (match.Success) {
                    scalarValue = string.IsNullOrEmpty(match.Groups["Path"].Value)
                        ? match.Result("{Binding RelativeSource={RelativeSource AncestorType=${AncestorType}}$'")
                        : match.Result("{Binding ${Path}, RelativeSource={RelativeSource AncestorType=${AncestorType}}$'");
                }
            }
            if (scalarValue == null) {
                match = ReBindingSource.Match(value);
                if (match.Success) {
                    scalarValue = string.IsNullOrEmpty(match.Groups["Path"].Value)
                        ? match.Result("{Binding Source=${Source}$'")
                        : match.Result("{Binding ${Path}, Source=${Source}$'");
                }
            }
            return scalarValue;
        }
    }
}