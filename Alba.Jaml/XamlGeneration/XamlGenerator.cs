using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public class XamlGenerator
    {
        private const string Dollar = "$";
        private const string Content = "_";
        private static readonly XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        private static readonly XNamespace nsX = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        private readonly JObject _data;
        private readonly string _nameSpace;
        private readonly string _className;

        public XamlGenerator (JObject data, string nameSpace, string className)
        {
            _data = data;
            _nameSpace = nameSpace;
            _className = className;
        }

        public string GenerateXaml ()
        {
            var root = GetXObject(_data);
            root.Add(
                //new XAttribute("xmlns", ns),
                new XAttribute(XNamespace.Xmlns + "x", nsX),
                new XAttribute(nsX + "Class", string.Format("{0}.{1}", _nameSpace, _className))
                );
            var xaml = new XDocument(root);

            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb,
                new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = true,
                    NamespaceHandling = NamespaceHandling.OmitDuplicates,
                }))
                xaml.Save(xmlWriter);
            return sb.ToString();
            //return xaml.ToString();
        }

        private XElement GetXObject (JObject jobj)
        {
            bool isRoot = jobj.Parent == null;
            string visibility, className, objName;
            ParseDollarField((string)jobj[Dollar], out visibility, out className, out objName);
            var xobj = new XElement(ns + className,
                // ClassModifier/FieldModifier="visibility"
                visibility == null ? null : new XAttribute(nsX + (isRoot ? "Class" : "Field") + "Modifier", visibility),
                // x:Name="objName"
                objName == null ? null : new XAttribute(nsX + "Name", objName),
                // attribute="scalarValue"
                jobj.Properties().Where(p => p.Name != Dollar && !p.Value.HasValues).Select(p =>
                    new XAttribute(FormatScalarPropertyName(p.Name), p.Value.ToString())),
                // <attribute>complexValue</attribute>
                jobj.Properties().Where(p => p.Name != Dollar && p.Name != Content && p.Value.HasValues).Select(p =>
                    new XElement(ns + FormatComplexPropertyName(p.Name, className),
                        p.Value.Cast<JObject>().Select(GetXObject)
                        )),
                // Content TODO put into appropriate properties, default to ContPropAttr
                jobj.Property(Content) == null ? null :
                    jobj.Property(Content).Value.Cast<JObject>().Select(GetXObject)
                );
            return xobj;
        }

        private static void ParseDollarField (string dollar, out string visibility, out string className, out string objName)
        {
            if (dollar == null) {
                visibility = null;
                className = "_";
                objName = null;
                return;
            }
            visibility = null;
            if (dollar.StartsWith("private"))
                visibility = "private";
            else if (dollar.StartsWith("protected"))
                visibility = "protected";
            else if (dollar.StartsWith("public"))
                visibility = "public";
            else if (dollar.StartsWith("internal"))
                visibility = "internal";
            if (visibility != null)
                dollar = dollar.Substring(visibility.Length + 1);
            int spacePos = dollar.IndexOf(' ');
            if (spacePos == -1) {
                className = dollar;
                objName = null;
            }
            else {
                className = dollar.Substring(0, spacePos);
                objName = dollar.Substring(spacePos + 1);
            }
        }

        private static string FormatScalarPropertyName (string name)
        {
            return name.Replace('$', '.');
        }

        private static string FormatComplexPropertyName (string name, string className)
        {
            name = FormatScalarPropertyName(name);
            return name.IndexOf('.') == -1 ? string.Format("{0}.{1}", className, name) : name;
        }
    }
}