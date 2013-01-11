using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private XElement GetXElementStyle (ObjectContext ctx)
        {
            List<JProperty> allProps = ctx.JObj.Properties().ToList();

            return new XElement(Ns + ctx.TypeName,
                GetXAttrObjectVisibility(ctx.JObj, ctx.Visibility),
                GetXAttrsObjectIds(ctx.ObjId, ctx.TypeInfo),
                allProps.Where(IsScalarProperty).Select(GetXAttrScalarProperty).ToArray(),
                allProps.Where(IsScalarContentProperty).Select(GetXTextScalarPropertyContent).ToArray(),
                allProps.Where(IsComplexProperty).Select(GetXElementComplexObjectProperty).ToArray(),
                ctx.JContent == null ? null : GetObjectOrEnum(ctx.JContent).Select(GetXObject).ToArray()
                );
        }
    }
}