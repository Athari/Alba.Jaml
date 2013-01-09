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
using Alba.Jaml.XamlGeneration.PropertyShortcuts;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public class XamlGenerator
    {
        public static readonly XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        public static readonly XNamespace nsX = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        private const string pnDollar = "$";
        private const string pnContent = "_";
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
            new[] { "{@=", "{DynamicResource " },
            new[] { "{@", "{StaticResource " },
            new[] { "{= ", "{Binding " },
            new[] { "{=", "{Binding" },
        };
        private static readonly Dictionary<Type, Type> DefaultItemTypes = new Dictionary<Type, Type> {
            { typeof(SetterBase), typeof(Setter) },
            { typeof(TriggerBase), typeof(Trigger) },
        };

        private readonly JObject _data;
        private readonly string _nameSpace;
        private readonly string _className;
        private readonly Dictionary<JToken, TokenTypeInfo> _typeInfos = new Dictionary<JToken, TokenTypeInfo>();
        private readonly List<IPropertyShortcut> PropertyShortcuts;

        public XamlGenerator (JObject data, string nameSpace, string className)
        {
            _data = data;
            _nameSpace = nameSpace;
            _className = className;
            PropertyShortcuts = GetType().Assembly.GetTypes()
                .Where(t => t.GetInterface(typeof(IPropertyShortcut).FullName) != null)
                .Select(t => (IPropertyShortcut)Activator.CreateInstance(t))
                .ToList();
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
            TokenTypeInfo parentInfo = GetTypeInfo(jobj.Parent);
            string visibility, typeName, objName;
            ParseDollarField((string)jobj[pnDollar], out visibility, out typeName, out objName);
            typeName = EnsureObjectTypeName(parentInfo, typeName);
            if (typeName == null)
                return null;

            JToken jcontent = jobj[pnContent];
            Func<JProperty, bool> isProperty = p => p.Name != pnDollar && p.Name != pnContent;
            Func<JProperty, bool> isScalarProperty = p => p.Name != pnDollar && p.Name != pnContent && !p.Value.HasValues;
            Func<JProperty, bool> isScalarContentProperty = p => p.Name == pnContent && !p.Value.HasValues;
            Func<JProperty, bool> isComplexProperty = p => p.Name != pnContent && p.Value.HasValues;

            _typeInfos[jobj] = new TokenTypeInfo(parentInfo.ItemType, null);
            foreach (JProperty prop in jobj.Properties().Where(isProperty))
                _typeInfos[prop] = new TokenTypeInfo(parentInfo.ItemType, prop.Name);

            var shortPropNames = new List<string>();
            var xAttrsShortProps = new List<XAttribute>();
            foreach (JProperty prop in jobj.Properties()) {
                IPropertyShortcut shortcut = PropertyShortcuts.FirstOrDefault(ps => ps.IsPropertySupported(prop));
                if (shortcut != null) {
                    shortPropNames.Add(prop.Name);
                    xAttrsShortProps.AddRange(shortcut.GetAttributes(prop));
                }
            }
            var allProps = jobj.Properties().Where(p => !shortPropNames.Contains(p.Name)).ToArray();

            return new XElement(ns + typeName,
                GetXAttrObjectVisibility(jobj, visibility),
                GetXAttrObjectName(parentInfo, objName),
                xAttrsShortProps,
                allProps.Where(isScalarProperty).Select(GetXAttrScalarProperty).ToArray(),
                allProps.Where(isScalarContentProperty).Select(GetXTextScalarPropertyContent).ToArray(),
                allProps.Where(isComplexProperty).Select(GetXElementComplexObjectProperty).ToArray(),
                jcontent == null ? null : GetObjectOrEnum(jcontent).Select(GetXObject).ToArray()
                );
        }

        private string EnsureObjectTypeName (TokenTypeInfo parentInfo, string typeName)
        {
            if (parentInfo.ItemType == null && typeName == null)
                //throw new InvalidOperationException("Object type not set." + jobj);
                return null;
            if (parentInfo.ItemType == null)
                parentInfo.ItemType = GetTypeByName(typeName);
            else if (typeName == null)
                typeName = parentInfo.ItemType.Name;
            return typeName;
        }

        private XAttribute GetXAttrObjectVisibility (JObject jobj, string visibility)
        {
            // x:ClassModifier/x:FieldModifier="visibility"
            bool isRoot = jobj.Parent == null;
            return visibility == null ? null : new XAttribute(nsX + (isRoot ? "Class" : "Field") + "Modifier", visibility);
        }

        private XAttribute GetXAttrObjectName (TokenTypeInfo parentInfo, string objName)
        {
            // x:Name/x:Key="objName"
            bool isContDict = parentInfo.ContType != null && IsTypeDictionary(parentInfo.ContType);
            return objName == null ? null : new XAttribute(nsX + (isContDict ? "Key" : "Name"), objName);
        }

        private XAttribute GetXAttrScalarProperty (JProperty prop)
        {
            // attribute="scalarValue"
            return new XAttribute(FormatScalarPropertyName(prop), FormatScalarPropertyValue(prop.Value.ToString()));
        }

        private XText GetXTextScalarPropertyContent (JProperty prop)
        {
            // <attribute>scalarValue</attribute>
            return new XText(prop.Value.ToString());
        }

        private XElement GetXElementComplexObjectProperty (JProperty prop)
        {
            // <attribute><complexValue/></attribute> TODO Single <complexValue/>?
            return new XElement(ns + FormatComplexPropertyName(prop),
                prop.Value.OfType<JObject>().Select(GetXObject)
                );
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

        private static Type GetPropertyItemType (Type objType, string propName)
        {
            Type propType = GetPropertyType(objType, propName);
            Type enumType = GetGenericInterface(propType, typeof(IEnumerable<>));
            Type itemType = null;
            if (enumType != null)
                itemType = enumType.GetGenericArguments()[0]; // T
            else {
                var dicType = GetGenericInterface(propType, typeof(IDictionary<,>));
                if (dicType != null)
                    itemType = dicType.GetGenericArguments()[1]; // TValue
            }
            if (itemType != null) {
                if (DefaultItemTypes.ContainsKey(itemType))
                    itemType = DefaultItemTypes[itemType];
                return itemType;
            }
            if (IsTypeDictionary(propType))
                return null;
            return propType;
        }

        private static Type GetPropertyType (Type objType, string propName)
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

        private string FormatScalarPropertyName (JProperty prop)
        {
            return prop.Name.Replace('$', '.');
        }

        private string FormatComplexPropertyName (JProperty prop)
        {
            string name = FormatScalarPropertyName(prop);
            return name.IndexOf('.') == -1 ? string.Format("{0}.{1}", GetTypeInfo(prop).ObjType.Name, name) : name;
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

        private static IEnumerable<JObject> GetObjectOrEnum (JToken jcontent)
        {
            return jcontent.Type == JTokenType.Object ? new[] { (JObject)jcontent } : jcontent.OfType<JObject>();
        }

        private TokenTypeInfo GetTypeInfo (JToken token)
        {
            if (token == null)
                return new TokenTypeInfo(null, null);
            if (token.Type == JTokenType.Array)
                token = token.Parent;
            TokenTypeInfo typeInfo;
            if (_typeInfos.TryGetValue(token, out typeInfo))
                return typeInfo;
            else
                return _typeInfos[token] = new TokenTypeInfo(null, null);
        }

        private class TokenTypeInfo
        {
            private readonly string _propName;
            private Type _itemType;
            private Type _contType;

            public TokenTypeInfo (Type objType, string propName)
            {
                ObjType = objType;
                _propName = propName;
            }

            public Type ObjType { get; private set; }

            public Type ItemType
            {
                get { return _itemType ?? (_itemType = ObjType == null ? null : GetPropertyItemType(ObjType, _propName)); }
                set { _itemType = value; }
            }

            public Type ContType
            {
                get { return _contType ?? (_contType = ObjType == null ? null : GetPropertyType(ObjType, _propName)); }
                //set { _contType = value; }
            }
        }
    }
}