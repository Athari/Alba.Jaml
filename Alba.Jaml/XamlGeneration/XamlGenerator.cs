using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration
{
    public class XamlGenerator
    {
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
            var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            var nsX = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

            var xaml = new XDocument(
                new XElement(ns + "Window",
                    new XAttribute("xmlns", ns),
                    new XAttribute(XNamespace.Xmlns + "x", nsX),
                    new XAttribute(nsX + "Class", string.Format("{0}.{1}", _nameSpace, _className)),
                    new XAttribute(nsX + "Name", "root"),
                    new XElement(ns + "Grid",
                        new XAttribute(nsX + "Name", "grdRoot"),
                        new XAttribute("ToolTip", "Grid!")
                        )
                    )
                );

            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            }))
                xaml.Save(xmlWriter);
            return sb.ToString();
            //return xaml.ToString();
        }
    }
}