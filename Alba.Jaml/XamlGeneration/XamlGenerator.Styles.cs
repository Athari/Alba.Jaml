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
                ((JContainer)jobj[pnContent]).Add(((JObject)jobj[pnSet]).Properties().Select(GetJObjectStyleSetter));
                jobj.Remove(pnSet);
            }
            if (jobj[pnOn] != null) {
                // TODO Add style triggers
                //jobj["Triggers"] = new JArray(((JObject)ctx.JContent).Properties().Select(GetJObjectStyleSetter));
                jobj.Remove(pnOn);
            }
        }

        private JObject GetJObjectStyleSetter (JProperty prop)
        {
            Match mWithTarget = ReSetterWithTarget.Match(prop.Name);
            string targetName = null, propName = prop.Name;
            if (mWithTarget.Success) {
                targetName = mWithTarget.Groups["TargetName"].Value;
                propName = mWithTarget.Groups["Name"].Value;
            }

            var jobjSetter = new JObject(new JProperty(pnDollar, "Setter"));
            if (targetName != null)
                jobjSetter.Add(new JProperty("TargetName", targetName));
            jobjSetter.Add(new JProperty("Property", propName));
            jobjSetter.Add(new JProperty("Value", prop.Value));
            return jobjSetter;
        }
    }
}