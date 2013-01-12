using System;
using System.Text;
using Alba.Jaml.MSInternal;
using Alba.Jaml.XamlGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alba.Test.Jaml.MSInternal
{
    [TestClass]
    public class MePullParserTest
    {
        //private static readonly Assembly PresentationCore = typeof(Visibility).Assembly;
        //private static readonly Assembly PresentationFramework = typeof(Window).Assembly;

        [TestMethod]
        public void MePullParser_Parse ()
        {
            const string me = "{Binding 1, 2, Path=Name, Converter={StaticResource MyRes}, ConverterParameter={x:Static Window.LeftProperty}}";
            var xamlGenerator = new XamlGenerator(null, null, null);
            var xamlSchema = new XamlSchemaContext( /*new[] { PresentationCore, PresentationFramework }*/);
            var xamlContext = new XamlParserContext(xamlSchema, GetType().Assembly);
            xamlContext.AddNamespacePrefix("", xamlGenerator.Ns.NamespaceName);
            xamlContext.AddNamespacePrefix("x", xamlGenerator.NsX.NamespaceName);
            var meParser = new MePullParser(xamlContext);
            var sb = new StringBuilder();
            foreach (XamlNode xamlNode in meParser.Parse(me, 0, 0))
                sb.AppendFormat("{0}\n", xamlNode);
            //throw new Exception(SR.Get("MissingImplicitPropertyTypeCase"));
            throw new Exception(sb.ToString());
        }
    }
}