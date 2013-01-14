using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using Alba.Jaml.MSInternal;
using Newtonsoft.Json.Linq;
using XamlLanguage = Alba.Jaml.MSInternal.XamlLanguage;

// ReSharper disable TooWideLocalVariableScope
namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private const string pnConverter = "Converter";

        private JToken GetJObjectMultiBinding (ConverterInfo conv, string afterExpr)
        {
            if (afterExpr.StartsWith(","))
                afterExpr = afterExpr.Substring(1).Trim();
            string strMultiBinding = string.Format("{{MultiBinding {0}}}", afterExpr);

            JObject xmlMultiBinding = GetJObjectFromMarkupExt(strMultiBinding);
            {
                xmlMultiBinding.Add(pnConverter, FormatGeneratedConverterReference(conv.Name));
                xmlMultiBinding.Add(pnContent, new JArray());
                {
                    var content = (JArray)xmlMultiBinding[pnContent];
                    foreach (string strSubBinding in conv.SubBindings)
                        content.Add(GetJObjectFromMarkupExt(strSubBinding));
                }
            }
            return xmlMultiBinding;
        }

        private JObject GetJObjectFromMarkupExt (string strMarkup)
        {
            JObject result = null;
            XamlNodeStackItem obj, member;
            var posParams = new List<string>();
            var stack = new List<XamlNodeStackItem>();

            foreach (XamlNode node in GetMarkupExtParser().Parse(strMarkup, 0, 0)) {
                switch (node.NodeType) {
                    case XamlNodeType.None:
                        break;

                    case XamlNodeType.StartObject:
                        string ns = FindPrefixByNamespace(node.XamlType.PreferredXamlNamespace);
                        stack.Add(new XamlNodeStackItem(node,
                            new JObject(new JProperty(pnDollar,
                                ns != "" ? string.Format("{0}:{1}", ns, node.XamlType.Name) : node.XamlType.Name))));
                        break;

                    case XamlNodeType.GetObject:
                        throw new NotImplementedException();

                    case XamlNodeType.EndObject:
                        obj = stack.Last();
                        if (stack.Count == 1)
                            result = obj.JObject;
                        stack.RemoveAt(stack.Count - 1);
                        if (stack.Count > 0) {
                            member = stack.Last();
                            if (member.JProperty != null)
                                member.JProperty.Value = FormatJObjectAsMarkupExt(obj.JObject);
                        }
                        break;

                    case XamlNodeType.StartMember:
                        obj = stack.Last(t => t.JObject != null);
                        member = new XamlNodeStackItem(node, new JProperty(node.Member.Name, null));
                        obj.JObject.Add(member.JProperty);
                        stack.Add(member);
                        break;

                    case XamlNodeType.EndMember:
                        member = stack.Last();
                        if (member.JProperty.Name == XamlLanguage.PositionalParameters.Name) {
                            obj = stack.Last(t => t.JObject != null);
                            member.JProperty.Remove();
                            AddPositionalParams(obj, posParams);
                            posParams.Clear();
                        }
                        stack.RemoveAt(stack.Count - 1);
                        break;

                    case XamlNodeType.Value:
                        member = stack.Last();
                        string value = string.Format(CultureInfo.InvariantCulture, "{0}", node.Value);
                        if (member.JProperty.Name == XamlLanguage.PositionalParameters.Name)
                            posParams.Add(value);
                        else
                            member.JProperty.Value = value;
                        break;

                    case XamlNodeType.NamespaceDeclaration:
                        throw new NotImplementedException();

                    default:
                        throw new InvalidOperationException();
                }
            }
            return result;
        }

        private string FormatJObjectAsMarkupExt (JObject jobj)
        {
            /*string xmlName = GetTypeByName(typeName).FullName.StartsWith(XamlLanguage.SWMNamespace)
                ? string.Format("{0}:{1}", NsXPrefix, jobj.Name.LocalName) : jobj.Name.LocalName;*/
            var typeName = (string)jobj[pnDollar];
            if (typeName.EndsWith(KnownStrings.Extension))
                typeName = typeName.Substring(0, typeName.Length - KnownStrings.Extension.Length);
            return jobj.HasValues
                ? string.Format("{{{0} {1}}}", typeName,
                    string.Join(", ", jobj.Properties().Where(p => p.Name != pnDollar).Select(a =>
                        string.Format(CultureInfo.InvariantCulture, "{0}={1}", a.Name, a.Value))))
                : string.Format("{{{0}}}", typeName);
        }

        private static void AddPositionalParams (XamlNodeStackItem obj, List<string> posParams)
        {
            Type objType = obj.Node.XamlType.UnderlyingType;
            ParameterInfo[] constrParams = objType.GetConstructors()
                .Select(ci => ci.GetParameters())
                .Single(prms => prms.Length == posParams.Count);
            for (int i = 0; i < constrParams.Length; i++) {
                string paramName = constrParams[i].Name;
                PropertyInfo paramProp = objType.GetProperties()
                    .SingleOrDefault(p => p.GetCustomAttribute<ConstructorArgumentAttribute>() != null &&
                        p.GetCustomAttribute<ConstructorArgumentAttribute>().ArgumentName == paramName);
                if (paramProp != null)
                    paramName = paramProp.Name;
                else
                    paramName = char.ToUpper(paramName[0]) + paramName.Substring(1);
                obj.JObject.Add(new JProperty(paramName, posParams[i]));
            }
        }

        private MePullParser GetMarkupExtParser ()
        {
            var xamlParserContext = new XamlParserContext(_xamlSchemaContext, GetType().Assembly);
            xamlParserContext.AddNamespacePrefix(NsPrefix, Ns.NamespaceName);
            xamlParserContext.AddNamespacePrefix(NsXPrefix, NsX.NamespaceName);
            return new MePullParser(xamlParserContext);
        }

        private string FindPrefixByNamespace (string ns)
        {
            if (ns == Ns.NamespaceName)
                return NsPrefix;
            else if (ns == NsX.NamespaceName)
                return NsXPrefix;
            throw new ArgumentException(string.Format("Invalid namespace: '{0}'.", ns), "ns");
        }

        private class XamlNodeStackItem
        {
            public XamlNode Node { get; set; }
            private JToken JToken { get; set; }
            public JObject JObject
            {
                get { return JToken as JObject; }
            }
            public JProperty JProperty
            {
                get { return JToken as JProperty; }
            }

            public XamlNodeStackItem (XamlNode node, JToken token)
            {
                Node = node;
                JToken = token;
            }
        }
    }
}