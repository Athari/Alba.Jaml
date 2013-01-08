using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public class XamlGenerator
    {
        private const string Dollar = "$";
        private const string Content = "_";
        private static readonly XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        private static readonly XNamespace nsX = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
        private static readonly Assembly PresentationCore = typeof(Visibility).Assembly;
        private static readonly Assembly PresentationFramework = typeof(Window).Assembly;
        private static readonly string[] WpfNameSpaces = new[] {
            "System.Windows",
            "System.Windows.Annotations",
            "System.Windows.Annotations.Storage",
            "System.Windows.Automation",
            "System.Windows.Automation.Peers",
            "System.Windows.Controls",
            "System.Windows.Controls.Primitives",
            "System.Windows.Data",
            "System.Windows.Documents",
            "System.Windows.Documents.DocumentStructures",
            "System.Windows.Documents.Serialization",
            "System.Windows.Ink",
            "System.Windows.Input",
            "System.Windows.Interop",
            "System.Windows.Markup",
            "System.Windows.Markup.Localizer",
            "System.Windows.Markup.Primitives",
            "System.Windows.Media",
            "System.Windows.Media.Animation",
            "System.Windows.Media.Composition",
            "System.Windows.Media.Converters",
            "System.Windows.Media.Effects",
            "System.Windows.Media.Imaging",
            "System.Windows.Media.Media3D",
            "System.Windows.Media.Media3D.Converters",
            "System.Windows.Media.TextFormatting",
            "System.Windows.Navigation",
            "System.Windows.Resources",
            "System.Windows.Shapes",
            "System.Windows.Shell",
        };

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
            var root = GetXObject(_data, null);
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

        private XElement GetXObject (JObject jobj, Type objType)
        {
            bool isRoot = jobj.Parent == null;
            string visibility, typeName, objName;
            ParseDollarField((string)jobj[Dollar], out visibility, out typeName, out objName);
            if (objType == null && typeName == null)
                //throw new InvalidOperationException("Object type not set." + jobj);
                return null;
            if (objType == null)
                objType = GetTypeByName(typeName);
            else if (typeName == null)
                typeName = objType.Name;

            var xobj = new XElement(ns + typeName,
                // ClassModifier/FieldModifier="visibility"
                visibility == null ? null : new XAttribute(nsX + (isRoot ? "Class" : "Field") + "Modifier", visibility),
                // x:Name="objName"
                objName == null ? null : new XAttribute(nsX + "Name", objName),
                // attribute="scalarValue"
                jobj.Properties().Where(p => p.Name != Dollar && !p.Value.HasValues).Select(p =>
                    new XAttribute(FormatScalarPropertyName(p.Name), p.Value.ToString())),
                // <attribute>complexValue</attribute>
                jobj.Properties().Where(p => p.Name != Content && p.Value.HasValues).Select(p =>
                    new XElement(ns + FormatComplexPropertyName(p.Name, typeName),
                        p.Value.Cast<JObject>().Select(o => GetXObject(o, GetPropertyItemType(objType, p.Name)))
                        )),
                // Content TODO put into appropriate properties, default to ContPropAttr
                jobj.Property(Content) == null ? null :
                    jobj.Property(Content).Value.Cast<JObject>().Select(o => GetXObject(o, null))
                );
            return xobj;
        }

        private Type GetPropertyItemType (Type objType, string propName)
        {
            var propType = GetPropertyType(objType, propName);
            var enumType = GetGenericInterface(propType, typeof(IEnumerable<>));
            if (enumType != null)
                return enumType.GetGenericArguments()[0]; // T
            var dicType = GetGenericInterface(propType, typeof(IDictionary<,>));
            if (dicType != null)
                return dicType.GetGenericArguments()[1]; // TValue
            if (objType.GetInterfaces().Any(it => it == typeof(IDictionary)))
                return typeof(object);
            return propType;
        }

        private Type GetPropertyType (Type objType, string propName)
        {
            PropertyInfo prop = objType.GetProperty(propName) ?? objType.GetProperty(propName + "Property");
            if (prop == null)
                throw new InvalidOperationException(string.Format("Property {0} not found in class {1}.",
                    propName, objType.FullName));
            return prop.PropertyType;
        }

        private Type GetTypeByName (string typeName)
        {
            Type type = GetWpfTypeByName(PresentationCore, typeName) ?? GetWpfTypeByName(PresentationFramework, typeName);
            if (type == null)
                throw new InvalidOperationException(string.Format("Class {0} not found.", typeName));
            return type;
        }

        private Type GetWpfTypeByName (Assembly assembly, string typeName)
        {
            return WpfNameSpaces.Select(space => assembly.GetType(string.Format("{0}.{1}", space, typeName))).FirstOrDefault(t => t != null);
        }

        private static void ParseDollarField (string dollar, out string visibility, out string typeName, out string objName)
        {
            if (dollar == null) {
                visibility = null;
                typeName = null;
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
                typeName = dollar;
                objName = null;
            }
            else {
                typeName = dollar.Substring(0, spacePos);
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

        private static Type GetGenericInterface (Type type, Type it)
        {
            return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == it);
        }
    }
}