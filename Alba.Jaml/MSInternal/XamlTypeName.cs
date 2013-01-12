using System;
using System.Collections.Generic;
using System.Xaml;

// ReSharper disable SuggestUseVarKeywordEvident
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable RedundantThisQualifier
namespace Alba.Jaml.MSInternal
{
    internal class XamlTypeName : System.Xaml.Schema.XamlTypeName
    {
        public XamlTypeName ()
        {}

        public XamlTypeName (string xamlNamespace, string name) : base(xamlNamespace, name)
        {}

        public XamlTypeName (string xamlNamespace, string name, IEnumerable<System.Xaml.Schema.XamlTypeName> typeArguments) : base(xamlNamespace, name, typeArguments)
        {}

        public XamlTypeName (XamlType xamlType) : base(xamlType)
        {}

        internal static XamlTypeName ParseInternal (string typeName, Func<string, string> prefixResolver, out string error)
        {
            XamlTypeName name = GenericTypeNameParser.ParseIfTrivalName(typeName, prefixResolver, out error);
            if (name != null) {
                return name;
            }
            GenericTypeNameParser parser = new GenericTypeNameParser(prefixResolver);
            return parser.ParseName(typeName, out error);
        }

        internal bool HasTypeArgs
        {
            get { return ((this.TypeArguments != null) && (this.TypeArguments.Count > 0)); }
        }

        public static XamlTypeName From (System.Xaml.Schema.XamlTypeName typeName)
        {
            return new XamlTypeName(typeName.Namespace, typeName.Name, typeName.TypeArguments);
        }
    }
}