using System;
using System.Linq;
using System.Text.RegularExpressions;
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
                var jsetters = ((JObject)jobj[pnSet]).Properties().Select(GetJObjectStyleSetter).ToArray();
                ((JContainer)jobj[pnContent]).Add(jsetters);

                Type targetType = GetTypeInfo(jobj).TargetType;
                foreach (JObject jsetter in jobj[pnContent]) {
                    TokenTypeInfo valueTypeInfo = GetTypeInfo(jsetter.Property("Value"));
                    valueTypeInfo.PropertyItemType = GetPropertyItemType(targetType, (string)jsetter["Property"]);
                }
                jobj.Remove(pnSet);
            }
            if (jobj[pnOn] != null) {
                // TODO Add style triggers
                //jobj["Triggers"] = new JArray(((JObject)ctx.JContent).Properties().Select(GetJObjectStyleSetter));
                jobj.Remove(pnOn);
            }
        }

        /// <summary>Convert JProperty to JObject style setter: &lt;Setter TargetName="targetName" Property="propName" Value="prop.Value"/&gt;.</summary>
        private JObject GetJObjectStyleSetter (JProperty prop)
        {
            // check for presence of ref.elementName
            string targetName = null, propName = FormatScalarPropertyName(prop);
            Match mWithTarget = ReSetterWithTarget.Match(propName);
            if (mWithTarget.Success) {
                targetName = mWithTarget.Groups["TargetName"].Value;
                propName = mWithTarget.Groups["PropName"].Value;
            }

            // create JObject setter
            var jsetter = new JObject(new JProperty(pnDollar, "Setter"));
            if (targetName != null)
                jsetter.Add(new JProperty("TargetName", targetName));
            jsetter.Add(new JProperty("Property", propName));
            jsetter.Add(new JProperty("Value", prop.Value));
            return jsetter;
        }
    }
}