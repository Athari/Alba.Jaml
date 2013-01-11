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

// ReSharper disable MemberCanBePrivate.Local
namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private readonly JObject _data;
        private readonly string _nameSpace;
        private readonly string _className;
        private readonly Dictionary<JToken, TokenTypeInfo> _typeInfos = new Dictionary<JToken, TokenTypeInfo>();
        private readonly List<ConverterInfo> _converters = new List<ConverterInfo>();
        private readonly List<IPropertyShortcut> _propertyShortcuts;

        public readonly XNamespace NsMy;

        public XamlGenerator (JObject data, string nameSpace, string className)
        {
            _data = data;
            _nameSpace = nameSpace;
            _className = className;
            _propertyShortcuts = GetType().Assembly.GetTypes()
                .Where(t => t.GetInterface(typeof(IPropertyShortcut).FullName) != null)
                .Select(t => (IPropertyShortcut)Activator.CreateInstance(t))
                .ToList();
            NsMy = String.Format("clr-namespace:{0}", _nameSpace);
        }

        public IEnumerable<ConverterInfo> Converters
        {
            get { return _converters; }
        }

        public string GenerateXaml ()
        {
            var root = GetXObject(_data);
            root.Add(
                //new XAttribute("xmlns", Ns),
                new XAttribute(XNamespace.Xmlns + "x", NsX),
                new XAttribute(XNamespace.Xmlns + "my", NsMy),
                new XAttribute(NsX + "Class", String.Format("{0}.{1}", _nameSpace, _className))
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
        }

        private XElement GetXObject (JObject jobj)
        {
            var ctx = new ObjectContext(this, jobj);
            if (ctx.TypeName == null)
                return null;

            if (ctx.TypeInfo.Type == typeof(Style))
                ProcessStyleObject(ctx);

            AssignPropertyTypeInfos(jobj, ctx);
            var allProps = new List<JProperty>();
            var xAttrsShortProps = GetXAttrsShortProps(jobj, allProps);

            return new XElement(Ns + ctx.TypeName,
                GetXAttrObjectVisibility(jobj, ctx.Visibility),
                GetXAttrsObjectIds(ctx.ObjId, ctx.TypeInfo),
                xAttrsShortProps,
                allProps.Where(IsScalarProperty).Select(GetXAttrScalarProperty).ToArray(),
                allProps.Where(IsScalarContentProperty).Select(GetXTextScalarPropertyContent).ToArray(),
                allProps.Where(IsComplexProperty).Select(GetXElementComplexObjectProperty).ToArray(),
                ctx.JContent == null ? null : GetObjectOrEnum(ctx.JContent).Select(GetXObject).ToArray()
                );
        }

        private List<XAttribute> GetXAttrsShortProps (JObject jobj, List<JProperty> allProps)
        {
            var shortPropNames = new List<string>();
            var xAttrsShortProps = new List<XAttribute>();
            foreach (JProperty prop in jobj.Properties()) {
                IPropertyShortcut shortcut = _propertyShortcuts.FirstOrDefault(ps => ps.IsPropertySupported(prop));
                if (shortcut != null) {
                    shortPropNames.Add(prop.Name);
                    xAttrsShortProps.AddRange(shortcut.GetAttributes(prop));
                }
            }
            allProps.AddRange(jobj.Properties().Where(p => !shortPropNames.Contains(p.Name)));
            return xAttrsShortProps;
        }

        private void AssignPropertyTypeInfos (JObject jobj, ObjectContext ctx)
        {
            _typeInfos[jobj] = ctx.TypeInfo;
            foreach (JProperty prop in jobj.Properties().Where(IsProperty))
                _typeInfos[prop] = new TokenTypeInfo { Type = ctx.TypeInfo.Type, PropName = prop.Name };
        }

        private XAttribute GetXAttrObjectVisibility (JObject jobj, string visibility)
        {
            // x:ClassModifier/x:FieldModifier="visibility"
            bool isRoot = jobj.Parent == null;
            return visibility == null ? null : new XAttribute(NsX + (isRoot ? "Class" : "Field") + "Modifier", visibility);
        }

        private IEnumerable<XAttribute> GetXAttrsObjectIds (string objId, TokenTypeInfo objInfo)
        {
            // x:Name/x:Key="objIdExplicit" ImplicitKey="objKeyImplicit"
            if (objId == null)
                yield break;
            string objIdExplicit = objId, objIdImplicit = null, propKey = GetObjectImplicitKeyPropName(objInfo);

            if (propKey != null) {
                int spacePos = objId.IndexOf(' ');
                if (spacePos != -1) {
                    objIdExplicit = objId.Substring(0, spacePos);
                    objIdImplicit = objId.Substring(spacePos + 1);
                }
                else {
                    objIdExplicit = null;
                    objIdImplicit = objId;
                }
                Type propKeyType = GetPropertyType(objInfo.Type, propKey);
                if (propKeyType == typeof(Type))
                    objIdImplicit = String.Format("{{x:Type {0}}}", objIdImplicit);
            }

            bool isContDict = objInfo.ContType != null && IsTypeDictionary(objInfo.ContType);
            if (objIdExplicit != null)
                yield return new XAttribute(NsX + (isContDict ? "Key" : "Name"), objIdExplicit);
            if (objIdImplicit != null)
                yield return new XAttribute(propKey, FormatScalarPropertyValue(objIdImplicit));
        }

        private static string GetObjectImplicitKeyPropName (TokenTypeInfo objInfo)
        {
            if (DictionaryKeyProps.ContainsKey(objInfo.Type))
                return DictionaryKeyProps[objInfo.Type];
            else {
                var attrDictKey = objInfo.Type.GetCustomAttributes<DictionaryKeyPropertyAttribute>().FirstOrDefault();
                if (attrDictKey != null)
                    return attrDictKey.Name;
            }
            return null;
        }

        private XText GetXTextScalarPropertyContent (JProperty prop)
        {
            // <attribute>scalarValue</attribute>
            return new XText(prop.Value.ToString());
        }

        private XElement GetXElementComplexObjectProperty (JProperty prop)
        {
            // <attribute><complexValue/></attribute> TODO Single <complexValue/>?
            return new XElement(Ns + FormatComplexPropertyName(prop),
                prop.Value.OfType<JObject>().Select(GetXObject)
                );
        }

        private Tuple<string, Type, Type> GetDefaultContentProperty (Type objType)
        {
            var attrContProp = objType.GetCustomAttribute<ContentPropertyAttribute>(true);
            if (attrContProp == null)
                throw new InvalidOperationException(String.Format("Content property for type {0} not found.", objType.FullName));
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
            throw new InvalidOperationException(String.Format("Property {0} not found in class {1}.",
                propName, objType.FullName));
        }

        private Type GetTypeByName (string typeName)
        {
            Type type = GetWpfTypeByName(PresentationCore, typeName)
                ?? GetWpfTypeByName(PresentationFramework, typeName)
                    ?? GetWpfTypeByName(PresentationCore, typeName + "Extension")
                        ?? GetWpfTypeByName(PresentationFramework, typeName + "Extension");
            if (type == null)
                throw new InvalidOperationException(String.Format("Class {0} not found.", typeName));
            return type;
        }

        private Type GetWpfTypeByName (Assembly assembly, string typeName)
        {
            return WpfNameSpaces.Select(space => assembly.GetType(String.Format("{0}.{1}", space, typeName))).FirstOrDefault(t => t != null);
        }

        private static void ParseDollarField (string dollar, out string visibility, out string typeName, out string objId)
        {
            if (dollar == null) {
                visibility = null;
                typeName = null;
                objId = null;
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
                objId = null;
            }
            else {
                typeName = dollar.Substring(0, spacePos);
                objId = dollar.Substring(spacePos + 1);
            }
        }

        private static string FormatScalarPropertyName (JProperty prop)
        {
            return prop.Name.Replace('$', '.');
        }

        private string FormatComplexPropertyName (JProperty prop)
        {
            string name = FormatScalarPropertyName(prop);
            return name.IndexOf('.') == -1 ? String.Format("{0}.{1}", GetTypeInfo(prop).Type.Name, name) : name;
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

        public static bool IsProperty (JProperty prop)
        {
            return prop.Name != pnDollar && prop.Name != pnContent;
        }

        public static bool IsScalarProperty (JProperty prop)
        {
            return prop.Name != pnDollar && prop.Name != pnContent && !prop.Value.HasValues;
        }

        public static bool IsScalarContentProperty (JProperty prop)
        {
            return prop.Name == pnContent && !prop.Value.HasValues;
        }

        public static bool IsComplexProperty (JProperty prop)
        {
            return prop.Name != pnContent && prop.Value.HasValues;
        }

        private TokenTypeInfo GetTypeInfo (JToken token)
        {
            if (token == null)
                return new TokenTypeInfo();
            if (token.Type == JTokenType.Array)
                token = token.Parent;
            TokenTypeInfo typeInfo;
            if (_typeInfos.TryGetValue(token, out typeInfo))
                return typeInfo;
            else
                return _typeInfos[token] = new TokenTypeInfo();
        }

        private class TokenTypeInfo
        {
            private Type _itemType;
            private Type _contType;

            public Type Type { get; set; }
            public Type ContType { get; set; }
            public string PropName { get; set; }

            public Type ItemType
            {
                get { return _itemType ?? (_itemType = Type == null ? null : GetPropertyItemType(Type, PropName)); }
            }

            public Type ItemContType
            {
                get { return _contType ?? (_contType = Type == null ? null : GetPropertyType(Type, PropName)); }
            }
        }

        private class ObjectContext
        {
            public ObjectContext (XamlGenerator generator, JObject jobj)
            {
                JObj = jobj;

                TokenTypeInfo parentInfo = generator.GetTypeInfo(JObj.Parent);
                TypeInfo = new TokenTypeInfo { Type = parentInfo.ItemType, ContType = parentInfo.ItemContType };

                string visibility, typeName, objId;
                ParseDollarField((string)JObj[pnDollar], out visibility, out typeName, out objId);
                Visibility = visibility;
                ObjId = objId;
                if (typeName != null) {
                    TypeInfo.Type = generator.GetTypeByName(typeName);
                    TypeName = typeName;
                }
                else if (TypeInfo.Type != null) {
                    TypeName = TypeInfo.Type.Name;
                }
            }

            public JObject JObj { get; private set; }
            public string ObjId { get; private set; }
            public TokenTypeInfo TypeInfo { get; private set; }
            public string Visibility { get; private set; }
            public string TypeName { get; private set; }

            public JToken JContent
            {
                get { return JObj[pnContent]; }
            }
        }
    }
}