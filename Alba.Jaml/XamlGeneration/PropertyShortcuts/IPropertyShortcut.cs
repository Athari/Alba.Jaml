using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration.PropertyShortcuts
{
    public interface IPropertyShortcut
    {
        bool IsPropertySupported (JProperty prop);
        IEnumerable<XAttribute> GetAttributes (JProperty prop);
    }
}