using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private const string ReIdent = @"[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}][\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        private const string ReCurlyBracesContents = @"
            (
                [^\{\}] | (?<paren>\{) | (?<-paren>\})
            )+
            (?(paren)(?!))";
        private const string ReInCurlyBraces = @"\{" + ReCurlyBracesContents + @"\}";
        private const string ReOptionalPropPath = @"(  \.  (?<Path>[^,\}]+)  )?";
        private const string StrExpressionSuffix = "${}";
        private const RegexOptions DefaultReOptions = RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
        private static readonly Regex ReBindingElementName = new Regex(
            @"^  \{=\s*  ref\.  (?<ElementName>" + ReIdent + @")  " + ReOptionalPropPath,
            DefaultReOptions);
        private static readonly Regex ReBindingSelf = new Regex(
            @"^  \{=\s*  this  " + ReOptionalPropPath,
            DefaultReOptions);
        private static readonly Regex ReBindingTemplatedParent = new Regex(
            @"^  \{=\s*  tpl  " + ReOptionalPropPath,
            DefaultReOptions);
        private static readonly Regex ReBindingAncestorType = new Regex(
            @"^  \{=\s*  ~  (?<AncestorType>[^,\.\}]+)  " + ReOptionalPropPath,
            DefaultReOptions);
        private static readonly Regex ReBindingSource = new Regex(
            @"^  \{=\s*  @  (?<Source>" + ReInCurlyBraces + @")  " + ReOptionalPropPath,
            DefaultReOptions);
        private static readonly Regex ReGenericBinding = new Regex(
            @"^  \{=\s*  (?<Expression>" + ReCurlyBracesContents + @")  \s*\}  $",
            DefaultReOptions);
        private static readonly Regex ReSubBinding = new Regex(
            @"  \$  (?<SubBinding>" + ReInCurlyBraces + @")  (?<Between>.*?)  (?= \$\{ | $ | ,(?!.*\$\{\}) )",
            DefaultReOptions);

        private object GetXAttrScalarProperty (JProperty prop)
        {
            // attribute="scalarValue"
            string value = prop.Value.ToString();
            if (!value.StartsWith("{="))
                return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(value));
            else
                return GetXBindingPropertyValue(prop);
        }

        private string FormatScalarPropertyValue (string value)
        {
            return MarkupAliases.Aggregate(value, (v, alias) => v.Replace(alias[0], alias[1]));
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

            //var conv = new ConverterInfo();
            return null;
        }

        public class ConverterInfo
        {}
    }
}