using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
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
        private static readonly string[][] MarkupAliases = new[] {
            new[] { "{@@", "{DynamicResource " },
            new[] { "{@", "{StaticResource " },
            new[] { "{=", "{Binding " },
        };
        private static readonly Dictionary<Type, Type> DefaultItemTypes = new Dictionary<Type, Type> {
            { typeof(SetterBase), typeof(Setter) },
            { typeof(TriggerBase), typeof(Trigger) },
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
            var root = GetXObject(_data, null, null);
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

        private XElement GetXObject (JObject jobj, Type objType, Type contType)
        {
            bool isRoot = jobj.Parent == null;
            bool isContDict = contType != null && IsTypeDictionary(contType);
            string visibility, typeName, objName;
            ParseDollarField((string)jobj[Dollar], out visibility, out typeName, out objName);

            if (objType == null && typeName == null)
                //throw new InvalidOperationException("Object type not set." + jobj);
                return null;
            if (objType == null)
                objType = GetTypeByName(typeName);
            else if (typeName == null)
                typeName = objType.Name;

            JToken jcontent = jobj[Content];
            var xobj = new XElement(ns + typeName,
                // x:ClassModifier/x:FieldModifier="visibility"
                visibility == null ? null : new XAttribute(nsX + (isRoot ? "Class" : "Field") + "Modifier", visibility),
                // x:Name/x:Key="objName"
                objName == null ? null : new XAttribute(nsX + (isContDict ? "Key" : "Name"), objName),
                // attribute="scalarValue"
                jobj.Properties().Where(p => p.Name != Dollar && p.Name != Content && !p.Value.HasValues).Select(p =>
                    new XAttribute(FormatScalarPropertyName(p.Name), FormatScalarPropertyValue(p.Value.ToString()))),
                // <attribute>scalarValue</attribute>
                jobj.Properties().Where(p => p.Name == Content && !p.Value.HasValues).Select(p =>
                    new XText(p.Value.ToString())),
                // <attribute><complexValue/></attribute>
                jobj.Properties().Where(p => p.Name != Content && p.Value.HasValues).Select(p =>
                    new XElement(ns + FormatComplexPropertyName(p.Name, typeName),
                        p.Value.OfType<JObject>().Select(o =>
                            GetXObject(o, GetPropertyItemType(objType, p.Name), GetPropertyType(objType, p.Name)))
                        )),
                // Content
                jcontent == null ? null :
                    (jcontent.Type == JTokenType.Object ? new[] { (JObject)jcontent } : jcontent.OfType<JObject>())
                        .Select(o => GetXObject(o, null, null))
                );
            return xobj;
        }

        private Tuple<string, Type, Type> GetDefaultContentProperty (Type objType)
        {
            var attrContProp = objType.GetCustomAttribute<ContentPropertyAttribute>(true);
            if (attrContProp == null)
                throw new InvalidOperationException(string.Format("Content property for type {0} not found.", objType.FullName));
            Type itemType = GetPropertyItemType(objType, attrContProp.Name);
            if (DefaultItemTypes.ContainsKey(itemType))
                itemType = DefaultItemTypes[itemType];
            Type contType = GetPropertyType(objType, attrContProp.Name);
            return new Tuple<string, Type, Type>(attrContProp.Name, itemType, contType);
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
            if (IsTypeDictionary(propType))
                return null;
            return propType;
        }

        private Type GetPropertyType (Type objType, string propName)
        {
            PropertyInfo prop = objType.GetProperty(propName);
            if (prop != null)
                return prop.PropertyType;
            FieldInfo dfield = objType.GetField(propName + "Property", BindingFlags.Static | BindingFlags.Public);
            if (dfield != null) {
                var dprop = dfield.GetValue(null) as DependencyProperty;
                if (dprop != null)
                    return dprop.PropertyType;
            }
            throw new InvalidOperationException(string.Format("Property {0} not found in class {1}.",
                propName, objType.FullName));
        }

        private Type GetTypeByName (string typeName)
        {
            Type type = GetWpfTypeByName(PresentationCore, typeName)
                ?? GetWpfTypeByName(PresentationFramework, typeName)
                    ?? GetWpfTypeByName(PresentationCore, typeName + "Extension")
                        ?? GetWpfTypeByName(PresentationFramework, typeName + "Extension");
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

        private static string FormatScalarPropertyValue (string value)
        {
            return MarkupAliases.Aggregate(value, (v, alias) => v.Replace(alias[0], alias[1]));
        }

        private static Type GetGenericInterface (Type type, Type it)
        {
            return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == it);
        }

        private static bool IsTypeDictionary (Type type)
        {
            return type.GetInterfaces().Any(it => it == typeof(IDictionary))
                || GetGenericInterface(type, typeof(IDictionary<,>)) != null;
        }
    }
}