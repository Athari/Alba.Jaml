using System.Collections.Generic;
using System.Reflection;
using System.Xaml;

namespace Alba.Jaml.MSInternal
{
    internal class XamlSchemaContext : System.Xaml.XamlSchemaContext
    {
        public XamlSchemaContext ()
        {}

        public XamlSchemaContext (XamlSchemaContextSettings settings) : base(settings)
        {}

        public XamlSchemaContext (IEnumerable<Assembly> referenceAssemblies) : base(referenceAssemblies)
        {}

        public XamlSchemaContext (IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings) : base(referenceAssemblies, settings)
        {}

        public XamlType PublicMorozov_GetXamlType (string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            return base.GetXamlType(xamlNamespace, name, typeArguments);
        }
    }
}