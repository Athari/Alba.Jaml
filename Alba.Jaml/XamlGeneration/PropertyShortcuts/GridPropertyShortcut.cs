using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Alba.Jaml.XamlGeneration.PropertyShortcuts
{
    public class GridPropertyShortcut : IPropertyShortcut
    {
        private const string GridPosPropName = "Grid$";
        private static readonly string[] GridPosSubPropNames = new[] {
            "Row", "Column", "RowSpan", "ColumnSpan"
        };

        public bool IsPropertySupported (JProperty prop)
        {
            return prop.Name == GridPosPropName;
        }

        public IEnumerable<XAttribute> GetAttributes (JProperty prop)
        {
            switch (prop.Name) {
                case GridPosPropName:
                    string[] propValues = ((string)prop.Value).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (propValues.Length < 1 || propValues.Length > 4)
                        throw new ArgumentException("From 1 to 4 values must be supplied.", "prop");
                    for (int i = 0; i < propValues.Length; i++)
                        yield return new XAttribute(string.Format("Grid.{0}", GridPosSubPropNames[i]), propValues[i]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("prop");
            }
        }
    }
}