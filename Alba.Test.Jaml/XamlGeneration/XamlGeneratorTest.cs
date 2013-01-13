using System.IO;
using Alba.Jaml.XamlGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Alba.Test.Jaml.XamlGeneration
{
    [TestClass]
    public class XamlGeneratorTest
    {
        [TestMethod]
        public void XamlGenerator_GenerateXaml_MainWin ()
        {
            var jobj = JObject.Parse(File.ReadAllText("../../../Alba.JamlTestApp/MainWin.jaml").Substring(2));
            var generator = new XamlGenerator(jobj, "Alba.Test.Jaml", "MainWin");
            generator.GenerateXaml();
        }
    }
}