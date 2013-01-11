using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private const string pnOn = "on";
        private static readonly Regex ReSetterWithTarget = new Regex(
            @"^  ref\.  (?<TargetName>" + ReIdent + @")  \.  (?<PropName>.+)  $",
            DefaultReOptions);

        private void ProcessStyleObject (ObjectContext ctx)
        {
            if (ctx.JContent != null)
                ctx.JObj[pnContent] = new JArray(((JObject)ctx.JContent).Properties().Select(GetJObjectStyleSetter));
            //if (ctx.JObj[pnOn] != null) {
            ctx.JObj.Remove(pnOn);
            //}
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