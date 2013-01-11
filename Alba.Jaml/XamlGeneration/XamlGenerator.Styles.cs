using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private const string pnSet = "set";
        private const string pnOn = "on";
        private static readonly Regex ReSetterWithTarget = new Regex(
            @"^  ref\.  (?<TargetName>" + ReIdent + @")  \.  (?<PropName>.+)  $",
            DefaultReOptions);

        private void ProcessStyleObject (ObjectContext ctx)
        {
            var jobj = ctx.JObj;
            if (jobj[pnSet] != null) {
                if (jobj[pnContent] == null)
                    jobj[pnContent] = new JArray();
                var jsetters = ((JObject)jobj[pnSet]).Properties().Select(p => GetJObjectStyleSetter(jobj, p)).ToArray();
                ((JContainer)jobj[pnContent]).Add(jsetters);

                Type targetType = GetTypeInfo(jobj).ForType;
                foreach (JObject jsetter in jobj[pnContent]) {
                    TokenTypeInfo valueTypeInfo = GetTypeInfo(jsetter.Property("Value"));
                    valueTypeInfo.ItemType = GetPropertyItemType(targetType, (string)jsetter["Property"]);
                }
                jobj.Remove(pnSet);
            }
            if (jobj[pnOn] != null) {
                // TODO Add style triggers
                //jobj["Triggers"] = new JArray(((JObject)ctx.JContent).Properties().Select(GetJObjectStyleSetter));
                jobj.Remove(pnOn);
            }
        }

        private JObject GetJObjectStyleSetter (JObject jobjStyle, JProperty prop)
        {
            string targetName = null, propName = FormatScalarPropertyName(prop);
            Match mWithTarget = ReSetterWithTarget.Match(propName);
            if (mWithTarget.Success) {
                targetName = mWithTarget.Groups["TargetName"].Value;
                propName = mWithTarget.Groups["PropName"].Value;
            }

            // <Setter TargetName="targetName" Property="propName" Value="prop.Value" />
            var jsetter = new JObject(new JProperty(pnDollar, "Setter"));
            if (targetName != null)
                jsetter.Add(new JProperty("TargetName", targetName));
            jsetter.Add(new JProperty("Property", propName));
            jsetter.Add(new JProperty("Value", prop.Value));
            return jsetter;
        }
    }
}