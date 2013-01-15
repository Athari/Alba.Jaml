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
        private const string pnTargetType = "TargetType";
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
                var jsetters = ((JObject)jstyle[pnSet]).Properties().Select(GetJObjectSetter).ToArray();
                ((JArray)jstyle[pnSetters]).Add(jsetters);
                AssignSetterTypes(jstyle, targetType);
                jstyle.Remove(pnSet);
            }

            if (jstyle[pnOn] != null) {
                if (jstyle[pnTriggers] == null)
                    jstyle[pnTriggers] = new JArray();
                var jtriggers = ((JObject)jstyle[pnOn]).Properties().Select(p => GetJObjectTrigger(p, targetType)).ToArray();
                ((JArray)jstyle[pnTriggers]).Add(jtriggers);
                jstyle.Remove(pnOn);
            }
        }

        private void ProcessTemplateObject (ObjectContext ctx)
        {
            JObject jtemplate = ctx.JObj;
            Type targetType = GetTypeInfo(jtemplate).TargetType;

            if (targetType == null) {
                var strTargetType = FormatScalarPropertyValue(jtemplate[pnTargetType]);
                targetType = GetTypeInfo(jtemplate).TargetType = GetTypeByName(strTargetType.StartsWith("{x:Type ")
                    ? strTargetType.Substring(8, strTargetType.Length - 9).Trim() : strTargetType);
            }

            if (jtemplate[pnOn] != null) {
                if (jtemplate[pnTriggers] == null)
                    jtemplate[pnTriggers] = new JArray();
                var jtriggers = ((JObject)jtemplate[pnOn]).Properties().Select(p => GetJObjectTrigger(p, targetType)).ToArray();
                ((JArray)jtemplate[pnTriggers]).Add(jtriggers);
                jtemplate.Remove(pnOn);
            }
        }

        private void AssignSetterTypes (JObject jstyle, Type targetType)
        {
            foreach (JObject jsetter in jstyle[pnSetters]) {
                TokenTypeInfo valueTypeInfo = GetTypeInfo(jsetter.Property(pnValue));
                valueTypeInfo.PropertyItemType = GetPropertyItemType(targetType, (string)jsetter[pnProperty]);
            }
        }

        /// <summary>Convert JProperty to JObject style setter: &lt;Setter TargetName="targetName" Property="propName" Value="prop.Value"/&gt;.</summary>
        private JObject GetJObjectSetter (JProperty prop)
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

        private JObject GetJObjectTrigger (JProperty prop, Type targetType)
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
                        var jsetters = ((JObject)prop.Value[pnSet]).Properties().Select(GetJObjectSetter).ToArray();
                        ((JArray)jtrigger[pnSetters]).Add(jsetters);
                        AssignSetterTypes(jtrigger, targetType);
                    }
                }
            }
            return jtrigger;
        }
    }
}