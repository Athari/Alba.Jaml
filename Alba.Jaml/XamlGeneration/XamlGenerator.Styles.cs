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
        private const string pnBinding = "Binding";
        private static readonly Regex ReSetterWithTarget = new Regex(
            @"^  ref\.  (?<TargetName>" + ReIdent + @")  \.  (?<PropName>.+)  $",
            DefaultReOptions);

        private void ProcessStyleObject (ObjectContext ctx)
        {
            JObject jstyle = ctx.JObj;
            Type targetType = GetTypeInfo(jstyle).TargetType;

            if (jstyle[pnSet] != null) {
                if (jstyle[pnSetters] == null)
                    jstyle[pnSetters] = new JArray();
                var jsetters = ((JObject)jstyle[pnSet]).Properties().Select(GetJObjectStyleSetter).ToArray();
                ((JContainer)jstyle[pnSetters]).Add(jsetters);
                AssignStyleSetterTypes(jstyle, targetType);
                jstyle.Remove(pnSet);
            }

            if (jstyle[pnOn] != null) {
                if (jstyle[pnTriggers] == null)
                    jstyle[pnTriggers] = new JArray();
                var jtriggers = ((JObject)jstyle[pnOn]).Properties().Select(p => GetJObjectStyleTrigger(p, targetType)).ToArray();
                ((JContainer)jstyle[pnTriggers]).Add(jtriggers);
                //jobj["Triggers"] = new JArray(((JObject)ctx.JContent).Properties().Select(GetJObjectStyleSetter));
                jstyle.Remove(pnOn);
            }
        }

        private void AssignStyleSetterTypes (JObject jstyle, Type targetType)
        {
            foreach (JObject jsetter in jstyle[pnSetters]) {
                TokenTypeInfo valueTypeInfo = GetTypeInfo(jsetter.Property(pnValue));
                valueTypeInfo.PropertyItemType = GetPropertyItemType(targetType, (string)jsetter[pnProperty]);
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

        private JObject GetJObjectStyleTrigger (JProperty prop, Type targetType)
        {
            var jtrigger = new JObject(new JProperty(pnDollar, "DataTrigger"));
            {
                var jbinding = new JProperty(pnBinding, prop.Name);
                ProcessBindingPropertyValue(jbinding);
                jtrigger.Add(jbinding);
                jtrigger.Add(pnValue, "True");
                if (prop.Value[pnSet] != null) {
                    jtrigger[pnSetters] = new JArray();
                    {
                        var jsetters = ((JObject)prop.Value[pnSet]).Properties().Select(GetJObjectStyleSetter).ToArray();
                        ((JContainer)jtrigger[pnSetters]).Add(jsetters);
                        AssignStyleSetterTypes(jtrigger, targetType);
                    }
                }
            }
            return jtrigger;
        }
    }
}