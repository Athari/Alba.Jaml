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
        private const string pnSetters = "Setters";
        private const string pnTriggers = "Triggers";
        private const string pnProperty = "Property";
        private const string pnValue = "Value";
        private const string pnTargetName = "TargetName";
        private static readonly Regex ReSetterWithTarget = new Regex(
            @"^  ref\.  (?<TargetName>" + ReIdent + @")  \.  (?<PropName>.+)  $",
            DefaultReOptions);

        private void ProcessStyleObject (ObjectContext ctx)
        {
            JObject jobj = ctx.JObj;
            Type targetType = GetTypeInfo(jobj).TargetType;

            if (jobj[pnSet] != null) {
                if (jobj[pnSetters] == null)
                    jobj[pnSetters] = new JArray();
                var jsetters = ((JObject)jobj[pnSet]).Properties().Select(GetJObjectStyleSetter).ToArray();
                ((JContainer)jobj[pnSetters]).Add(jsetters);
                foreach (JObject jsetter in jobj[pnSetters]) {
                    TokenTypeInfo valueTypeInfo = GetTypeInfo(jsetter.Property(pnValue));
                    valueTypeInfo.PropertyItemType = GetPropertyItemType(targetType, (string)jsetter[pnProperty]);
                }
                jobj.Remove(pnSet);
            }

            if (jobj[pnOn] != null) {
                if (jobj[pnTriggers] == null)
                    jobj[pnTriggers] = new JArray();
                var jtriggers = ((JObject)jobj[pnOn]).Properties().Select(GetJObjectStyleTrigger).ToArray();
                ((JContainer)jobj[pnTriggers]).Add(jtriggers);
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
            {
                if (targetName != null)
                    jsetter.Add(new JProperty(pnTargetName, targetName));
                jsetter.Add(new JProperty(pnProperty, propName));
                jsetter.Add(new JProperty(pnValue, prop.Value));
            }
            return jsetter;
        }

        private object GetJObjectStyleTrigger (JProperty prop)
        {
            return null;
        }
    }
}