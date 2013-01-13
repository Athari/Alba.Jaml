using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using Alba.Jaml.MSInternal;
using XamlLanguage = Alba.Jaml.MSInternal.XamlLanguage;

// ReSharper disable TooWideLocalVariableScope
namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        private XElement GetXElementMultiBinding (ConverterInfo conv, string afterExpr)
        {
            if (afterExpr.StartsWith(","))
                afterExpr = afterExpr.Substring(1).Trim();
            string strMultiBinding = string.Format("{{MultiBinding {0}}}", afterExpr);

            XElement xmlMultiBinding = GetXElementFromMarkupExt(strMultiBinding);
            xmlMultiBinding.Add(new XAttribute("Converter", FormatGeneratedConverterReference(conv.Name)));
            foreach (string strSubBinding in conv.SubBindings)
                xmlMultiBinding.Add(GetXElementFromMarkupExt(strSubBinding));
            return xmlMultiBinding;
        }

        private XElement GetXElementFromMarkupExt (string strMarkup)
        {
            XElement result = null;
            XamlNodeStackItem obj, member;
            var posParams = new List<string>();
            var stack = new List<XamlNodeStackItem>();

            if (_xamlParserContext.FindNamespaceByPrefix(NsPrefix) == null)
                _xamlParserContext.AddNamespacePrefix(NsPrefix, Ns.NamespaceName);
            if (_xamlParserContext.FindNamespaceByPrefix(NsXPrefix) == null)
                _xamlParserContext.AddNamespacePrefix(NsXPrefix, NsX.NamespaceName);
            var markupExtParser = new MePullParser(_xamlParserContext);

            foreach (XamlNode node in markupExtParser.Parse(strMarkup, 0, 0)) {
                switch (node.NodeType) {
                    case XamlNodeType.None:
                        break;

                    case XamlNodeType.StartObject:
                        stack.Add(new XamlNodeStackItem(node,
                            new XElement(XNamespace.Get(node.XamlType.PreferredXamlNamespace) + node.XamlType.Name)));
                        break;

                    case XamlNodeType.GetObject:
                        throw new NotImplementedException();

                    case XamlNodeType.EndObject:
                        obj = stack.Last();
                        if (stack.Count == 1)
                            result = obj.XElement;
                        stack.RemoveAt(stack.Count - 1);
                        if (stack.Count > 0) {
                            member = stack.Last();
                            if (member.XAttribute != null)
                                member.XAttribute.Value = FormatXElementAsMarkupExt(obj.XElement);
                        }
                        break;

                    case XamlNodeType.StartMember:
                        obj = stack.Last(t => t.XElement != null);
                        member = new XamlNodeStackItem(node, new XAttribute(node.Member.Name, ""));
                        obj.XElement.Add(member.XAttribute);
                        stack.Add(member);
                        break;

                    case XamlNodeType.EndMember:
                        member = stack.Last();
                        if (member.XAttribute.Name == XamlLanguage.PositionalParameters.Name) {
                            obj = stack.Last(t => t.XElement != null);
                            member.XAttribute.Remove();
                            AddPositionalParams(obj, posParams);
                            posParams.Clear();
                        }
                        stack.RemoveAt(stack.Count - 1);
                        break;

                    case XamlNodeType.Value:
                        member = stack.Last();
                        string value = string.Format(CultureInfo.InvariantCulture, "{0}", node.Value);
                        if (member.XAttribute.Name == XamlLanguage.PositionalParameters.Name)
                            posParams.Add(value);
                        else
                            member.XAttribute.Value = value;
                        break;

                    case XamlNodeType.NamespaceDeclaration:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        private string FormatXElementAsMarkupExt (XElement xml)
        {
            string xmlName = GetTypeByName(xml.Name.LocalName).FullName.StartsWith(XamlLanguage.SWMNamespace)
                ? string.Format("{0}:{1}", NsXPrefix, xml.Name.LocalName) : xml.Name.LocalName;
            if (xmlName.EndsWith(KnownStrings.Extension))
                xmlName = xmlName.Substring(0, xmlName.Length - KnownStrings.Extension.Length);
            return xml.HasAttributes
                ? string.Format("{{{0} {1}}}", xmlName,
                    string.Join(", ", xml.Attributes().Select(a =>
                        string.Format(CultureInfo.InvariantCulture, "{0}={1}",
                            a.Name.LocalName, a.Value))))
                : string.Format("{{{0}}}", xmlName);
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
                obj.XElement.Add(new XAttribute(paramName, posParams[i]));
            }
        }

        private class XamlNodeStackItem
        {
            public XamlNode Node { get; set; }
            private object XItem { get; set; }
            public XElement XElement
            {
                get { return XItem as XElement; }
            }
            public XAttribute XAttribute
            {
                get { return XItem as XAttribute; }
            }

            public XamlNodeStackItem (XamlNode node, object xItem)
            {
                Node = node;
                XItem = xItem;
            }
        }
    }
}