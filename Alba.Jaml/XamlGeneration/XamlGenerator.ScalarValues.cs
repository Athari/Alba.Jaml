using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Alba.Jaml.MSInternal;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        /// <summary>Regex part: C# identifier.</summary>
        private const string ReIdent = @"[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}][\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        /// <summary>Regex part: match only properly paired curly brackets.</summary>
        private const string ReCurlyBracesContents = @"
            (
                [^\{\}] | (?<paren>\{) | (?<-paren>\})
            )+
            (?(paren)(?!))";
        /// <summary>Regex part: match expression in curly brackets, with brackets properly paired.</summary>
        private const string ReInCurlyBraces = @"\{" + ReCurlyBracesContents + @"\}";
        /// <summary>Regex part: binding path.</summary>
        private const string ReOptionalPropPath = @"(  \.  (?<Path>[^,\}]+)  )?";
        /// <summary>Regex part: postfix in case of converter expression with commas after last sub-binding.</summary>
        private const string StrExpressionPostfix = "${}";

        private const RegexOptions DefaultReOptions = RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
        /// <summary>Examples: {=ref.elementName.PropertyPath}, {=ref.elementName}.</summary>
        private static readonly Regex ReBindingElementName = new Regex(
            @"^  \{=\s*  ref\.  (?<ElementName>" + ReIdent + @")  " + ReOptionalPropPath,
            DefaultReOptions);
        /// <summary>Examples: {=self.PropertyPath}, {=self}.</summary>
        private static readonly Regex ReBindingSelf = new Regex(
            @"^  \{=\s*  this  " + ReOptionalPropPath,
            DefaultReOptions);
        /// <summary>Examples: {=tpl.PropertyPath}, {=tpl}.</summary>
        private static readonly Regex ReBindingTemplatedParent = new Regex(
            @"^  \{=\s*  tpl  " + ReOptionalPropPath,
            DefaultReOptions);
        /// <summary>Examples: {=~AncestorType.PropertyPath}, {=~AncestorType}.</summary>
        private static readonly Regex ReBindingAncestorType = new Regex(
            @"^  \{=\s*  ~  (?<AncestorType>[^,\.\}]+)  " + ReOptionalPropPath,
            DefaultReOptions);
        /// <summary>Examples: {=@{source}.PropertyPath}, {=@{source}}.</summary>
        private static readonly Regex ReBindingSource = new Regex(
            @"^  \{=\s*  @  (?<Source>" + ReInCurlyBraces + @")  " + ReOptionalPropPath,
            DefaultReOptions);
        /// <summary>Example: {=...}.</summary>
        private static readonly Regex ReGenericBinding = new Regex(
            @"^  \{=\s*  (?<Expression>" + ReCurlyBracesContents + @")  \s*\}  $",
            DefaultReOptions);
        /// <summary>Match sub-cinding in curly braces, match after until either another sub-binding,
        /// or end of string, or comma (if ${} postfix is not present).</summary>
        private static readonly Regex ReSubBinding = new Regex(
            @"  \$  (?<SubBinding>" + ReInCurlyBraces + @")  (?<Between>.*?)  (?= \$\{ | $ | ,(?!.*\$\{\}) )",
            DefaultReOptions);

        /// <summary>Get XAttribute for scalar property: attribute="scalarValue" (can also return object if value is multi-binding).</summary>
        private object GetXAttrScalarProperty (JProperty prop)
        {
            if (prop.Value.Type == JTokenType.String && prop.Value.ToString().StartsWith("{="))
                return GetXBindingPropertyValue(prop);
            else
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(prop.Value));
        }

        private string FormatScalarPropertyValue (JToken value)
        {
            switch (value.Type) {
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return ((JValue)value).ToString(CultureInfo.InvariantCulture);
                case JTokenType.String:
                    return MarkupAliases.Aggregate((string)value, (v, alias) => v.Replace(alias[0], alias[1]));
                case JTokenType.Null:
                    return "{x:Null}";
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        private object GetXBindingPropertyValue (JProperty prop)
        {
            string value = prop.Value.ToString();

            string scalarValue = FormatSimpleBindingScalarValue(value);
            if (scalarValue != null)
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(scalarValue));

            object complexValue = FormatComplexBindingScalarValue(value);
            if (complexValue == null)
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(value));
            else if (complexValue is string)
                return new XAttribute(FormatScalarPropertyName(prop), complexValue);
            else
                return new XElement(Ns + FormatComplexPropertyName(prop), complexValue);
        }

        private string FormatSimpleBindingScalarValue (string value)
        {
            Match mBinding = ReBindingElementName.Match(value);
            if (mBinding.Success) {
                return String.IsNullOrEmpty(mBinding.Groups["Path"].Value)
                    ? mBinding.Result("{Binding ElementName=${ElementName}$'")
                    : mBinding.Result("{Binding ${Path}, ElementName=${ElementName}$'");
            }
            mBinding = ReBindingSelf.Match(value);
            if (mBinding.Success) {
                return String.IsNullOrEmpty(mBinding.Groups["Path"].Value)
                    ? mBinding.Result("{Binding RelativeSource={RelativeSource Self}$'")
                    : mBinding.Result("{Binding ${Path}, RelativeSource={RelativeSource Self}$'");
            }
            mBinding = ReBindingTemplatedParent.Match(value);
            if (mBinding.Success) {
                return String.IsNullOrEmpty(mBinding.Groups["Path"].Value)
                    ? mBinding.Result("{Binding RelativeSource={RelativeSource TemplatedParent}$'")
                    : mBinding.Result("{Binding ${Path}, RelativeSource={RelativeSource TemplatedParent}$'");
            }
            mBinding = ReBindingAncestorType.Match(value);
            if (mBinding.Success) {
                return String.IsNullOrEmpty(mBinding.Groups["Path"].Value)
                    ? mBinding.Result("{Binding RelativeSource={RelativeSource AncestorType=${AncestorType}}$'")
                    : mBinding.Result("{Binding ${Path}, RelativeSource={RelativeSource AncestorType=${AncestorType}}$'");
            }
            mBinding = ReBindingSource.Match(value);
            if (mBinding.Success) {
                return String.IsNullOrEmpty(mBinding.Groups["Path"].Value)
                    ? mBinding.Result("{Binding Source=${Source}$'")
                    : mBinding.Result("{Binding ${Path}, Source=${Source}$'");
            }
            return null;
        }

        private object FormatComplexBindingScalarValue (string value)
        {
            var mBinding = ReGenericBinding.Match(value);
            if (!mBinding.Success)
                return null;

            var mSubBindings = ReSubBinding.Matches(mBinding.Groups["Expression"].Value);
            if (mSubBindings.Count == 0)
                return null;

            var conv = new ConverterInfo { Name = "_jaml_Converter" };
            EnsureConverterNameUnique(conv);
            _converters.Add(conv);
            return null;
        }

        private void EnsureConverterNameUnique (ConverterInfo conv)
        {
            string name = conv.Name;
            int num = 1;
            while (_converters.Any(c => c.Name == conv.Name))
                conv.Name = name + num++;
        }

        public class ConverterInfo
        {
            public string Name { get; set; }
        }
    }
}