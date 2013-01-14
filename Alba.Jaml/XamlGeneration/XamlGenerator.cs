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
using Alba.Jaml.MSInternal;
using Alba.Jaml.XamlGeneration.PropertyShortcuts;
using Newtonsoft.Json.Linq;

// TODO TargetType autodection for Style[Targetype=Button].Setter.Value.Template
// TODO Replace HasValues check with type check

// ReSharper disable MemberCanBePrivate.Local
namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private readonly JObject _data;
        private readonly Dictionary<JToken, TokenTypeInfo> _typeInfos = new Dictionary<JToken, TokenTypeInfo>();
        private readonly List<IPropertyShortcut> _propertyShortcuts;
        private readonly XamlSchemaContext _xamlSchemaContext;

        public readonly XNamespace NsMy;

        public XamlGenerator (JObject data, string nameSpace, string className)
        {
            _data = data;
            NameSpace = nameSpace;
            ClassName = className;
            Converters = new List<ConverterInfo>();
            NsMy = String.Format("{0}:{1}", KnownStrings.UriClrNamespace, NameSpace);

            _xamlSchemaContext = new XamlSchemaContext( /*new[] { PresentationCore, PresentationFramework }*/);

            _propertyShortcuts = GetType().Assembly.GetTypes()
                .Where(t => t.GetInterface(typeof(IPropertyShortcut).FullName) != null)
                .Select(t => (IPropertyShortcut)Activator.CreateInstance(t))
                .ToList();
        }

        public string ClassName { get; private set; }
        public string ClassVisibility { get; private set; }
        public string NameSpace { get; private set; }
        public List<ConverterInfo> Converters { get; private set; }

        public string GenerateXaml ()
        {
            var root = GetXObject(_data);
            root.Add(
                new XAttribute("xmlns", Ns),
                new XAttribute(XNamespace.Xmlns + NsXPrefix, NsX),
                new XAttribute(XNamespace.Xmlns + NsLocalPrefix, NsMy),
                new XAttribute(NsX + XamlLanguage.Class.Name, String.Format("{0}.{1}", NameSpace, ClassName))
                );
            var xaml = new XDocument(root);
            var attrClassVisibility = root.Attribute(NsX + XamlLanguage.ClassModifier.Name);
            ClassVisibility = attrClassVisibility != null ? attrClassVisibility.Value : "public";

            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb,
                new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = IndentChars,
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

            var xAttrVisibility = GetXAttrObjectVisibility(jobj, ctx.Visibility);
            var xAttrsIds = GetXAttrsObjectIds(ctx.ObjId, ctx.TypeInfo).ToArray();

            if (ctx.TypeInfo.Type == typeof(Style))
                ProcessStyleObject(ctx);

            AssignPropertyTypeInfos(jobj, ctx);
            var allProps = new List<JProperty>();
            var xAttrsShortProps = GetXAttrsShortProps(jobj, allProps);

            return new XElement(Ns + ctx.TypeName,
                xAttrVisibility,
                xAttrsIds,
                xAttrsShortProps,
                allProps.Where(IsComplexProperty).Select(GetXElementComplexProperty).ToArray(),
                allProps.Where(IsScalarProperty).Select(GetXAttrScalarProperty).ToArray(),
                allProps.Where(IsScalarContentProperty).Select(GetXTextScalarPropertyContent).ToArray(),
                ctx.JContent == null ? null : GetObjectOrEnum(ctx.JContent).Select(GetXObject).ToArray()
                );
        }

        /// <summary>Get XAttributes returned by implementations of IPropertyShortcut.</summary>
        /// <param name="jobj">Processed object.</param>
        /// <param name="allProps">[out] All properties except processed by shortcuts.</param>
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

        /// <summary>Assign real type information for object properties.</summary>
        private void AssignPropertyTypeInfos (JObject jobj, ObjectContext ctx)
        {
            foreach (JProperty prop in jobj.Properties().Where(IsProperty)) {
                TokenTypeInfo typeInfo = GetTypeInfo(prop);
                if (typeInfo.Type == null)
                    typeInfo.Type = ctx.TypeInfo.Type;
                if (typeInfo.PropertyName == null)
                    typeInfo.PropertyName = prop.Name;
            }
        }

        /// <summary>Get XAttribute for visibility modifier: x:ClassModifier/x:FieldModifier="visibility".</summary>
        private XAttribute GetXAttrObjectVisibility (JObject jobj, string visibility)
        {
            bool isRoot = jobj.Parent == null;
            return visibility == null ? null : new XAttribute(
                NsX + (isRoot ? XamlLanguage.ClassModifier.Name : XamlLanguage.FieldModifier.Name),
                visibility);
        }

        /// <summary>Get XAttributes for object identifiers: x:Name/x:Key="objIdExplicit" ImplicitKey="objKeyImplicit".</summary>
        private IEnumerable<XAttribute> GetXAttrsObjectIds (string objId, TokenTypeInfo objInfo)
        {
            if (objId == null)
                yield break;
            string objIdExplicit = objId, objIdImplicit = null, propKey = GetObjectImplicitKeyPropName(objInfo), typeName = null;

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
                if (propKeyType == typeof(Type)) {
                    typeName = objIdImplicit;
                    objIdImplicit = String.Format("{{x:Type {0}}}", objIdImplicit);
                }
            }

            if (typeName != null)
                objInfo.TargetType = GetTypeByName(typeName);

            bool isContDict = objInfo.PropertyContainerType != null && IsTypeDictionary(objInfo.PropertyContainerType);
            if (objIdExplicit != null)
                yield return new XAttribute(
                    NsX + (isContDict ? XamlLanguage.Key.Name : XamlLanguage.Name.Name),
                    objIdExplicit);
            if (objIdImplicit != null)
                yield return new XAttribute(propKey, FormatScalarPropertyValue(objIdImplicit));
        }

        /// <summary>Get implicit key property name: TargetType for Style, DataType for DataTemplate etc.</summary>
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

        /// <summary>Get XText for scalar properties: &lt;attribute&gt;scalarValue&lt;/attribute&gt;.</summary>
        private XText GetXTextScalarPropertyContent (JProperty prop)
        {
            return new XText(prop.Value.ToString());
        }

        /// <summary>Get XElement for complex properties: &lt;attribute&gt;&lt;complexValue/&gt;&lt;/attribute&gt;.</summary>
        private XElement GetXElementComplexProperty (JProperty prop)
        {
            return new XElement(Ns + FormatComplexPropertyName(prop),
                GetObjectOrEnum(prop.Value).Select(GetXObject)
                );
        }

        /// <summary>Get types of objects contained in property: type of property for simple property,
        /// T for IEnumerable, TValue for IDictionary.</summary>
        private Type GetPropertyItemType (Type objType, string propName)
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

        /// <summary>Get property type by object type and property name. Supports simple, dependency and attached properties.</summary>
        private Type GetPropertyType (Type objType, string propName)
        {
            // attached property
            int dotPos = propName.IndexOf('.');
            if (dotPos != -1)
                return GetPropertyType(GetTypeByName(propName.Substring(0, dotPos)), propName.Substring(dotPos + 1));

            // simple property
            PropertyInfo prop = objType.GetProperty(propName);
            if (prop != null)
                return prop.PropertyType;

            // dependency property
            FieldInfo dfield = objType.GetField(propName + KnownStrings.DependencyPropertySuffix,
                BindingFlags.Static | BindingFlags.Public);
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
                    ?? GetWpfTypeByName(PresentationCore, typeName + KnownStrings.Extension)
                        ?? GetWpfTypeByName(PresentationFramework, typeName + KnownStrings.Extension);
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
            return name.IndexOf('.') == -1 ? string.Format("{0}.{1}", GetTypeInfo(prop).Type.Name, name) : name;
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
                return new TokenTypeInfo(this);
            if (token.Type == JTokenType.Array)
                token = token.Parent;
            TokenTypeInfo typeInfo;
            if (_typeInfos.TryGetValue(token, out typeInfo))
                return typeInfo;
            else
                return _typeInfos[token] = new TokenTypeInfo(this);
        }

        /// <summary>Real type info for JTokens.</summary>
        private class TokenTypeInfo
        {
            private readonly XamlGenerator _generator;
            private Type _itemType;
            private Type _contType;

            public TokenTypeInfo (XamlGenerator generator)
            {
                _generator = generator;
            }

            /// <summary>For objects: type of the object; for properties: type of the container object.</summary>
            public Type Type { get; set; }
            /// <summary>For objects: contained within collection of type.</summary>
            public Type PropertyContainerType { get; set; }
            /// <summary>For types with implicit keys (styles, templates): TargetType.</summary>
            public Type TargetType { get; set; }
            /// <summary>For properties: name of the property.</summary>
            public string PropertyName { get; set; }
            /// <summary>For properties: contains items of type (usually item.parent.PropertyItemType == item.Type).</summary>
            public Type PropertyItemType
            {
                get { return Type == null ? null : _itemType ?? (_itemType = _generator.GetPropertyItemType(Type, PropertyName)); }
                set { _itemType = value; }
            }
            /// <summary>For properties: type of property (usually item.parent.PropertyType == item.PropertyContainerType).</summary>
            public Type PropertyType
            {
                get { return Type == null ? null : _contType ?? (_contType = _generator.GetPropertyType(Type, PropertyName)); }
            }
        }

        /// <summary>Context for JObject processing.</summary>
        private class ObjectContext
        {
            public ObjectContext (XamlGenerator generator, JObject jobj)
            {
                JObj = jobj;

                TokenTypeInfo parentInfo = generator.GetTypeInfo(JObj.Parent);
                TypeInfo = generator.GetTypeInfo(JObj);
                if (TypeInfo.Type == null)
                    TypeInfo.Type = parentInfo.PropertyItemType;
                if (TypeInfo.PropertyContainerType == null)
                    TypeInfo.PropertyContainerType = parentInfo.PropertyType;

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

            /// <summary>Context is for this JObject.</summary>
            public JObject JObj { get; private set; }
            /// <summary>Object identifier: x:Key or x:Name.</summary>
            public string ObjId { get; private set; }
            /// <summary>Real type information for object.</summary>
            public TokenTypeInfo TypeInfo { get; private set; }
            /// <summary>Visibility modifier for object: x:ClassModifier or x:FieldModifier.</summary>
            public string Visibility { get; private set; }
            /// <summary>Short type name of object.</summary>
            public string TypeName { get; private set; }
            /// <summary>Value of content property of object.</summary>
            public JToken JContent
            {
                get { return JObj[pnContent]; }
            }
        }
    }
}