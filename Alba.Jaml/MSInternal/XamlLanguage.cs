using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml.Serialization;

// ReSharper disable RedundantThisQualifier
// ReSharper disable SuggestUseVarKeywordEvident
// ReSharper disable RedundantNameQualifier
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable RedundantDelegateCreation
// ReSharper disable CheckForReferenceEqualityInstead.1
namespace Alba.Jaml.MSInternal
{
    /// <summary>Defines constants that provide strings or objects that are useful for XAML markup or for programming with XAML types. These strings or objects are relevant to XAML language concepts, to the implementation of XAML language concepts in .NET Framework XAML Services, or to both.</summary>
    public static class XamlLanguage
    {
        internal const string PreferredPrefix = "x";
        private static Lazy<ReadOnlyCollection<XamlDirective>> s_allDirectives = new Lazy<ReadOnlyCollection<XamlDirective>>(new Func<ReadOnlyCollection<XamlDirective>>(XamlLanguage.GetAllDirectives));
        private static Lazy<ReadOnlyCollection<XamlType>> s_allTypes = new Lazy<ReadOnlyCollection<XamlType>>(new Func<ReadOnlyCollection<XamlType>>(XamlLanguage.GetAllTypes));
        private static Lazy<XamlDirective> s_arguments = new Lazy<XamlDirective>(() => GetXamlDirective("Arguments", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_array = new Lazy<XamlType>(() => GetXamlType(typeof(ArrayExtension)));
        private static Lazy<XamlDirective> s_asyncRecords = new Lazy<XamlDirective>(() => GetXamlDirective("AsyncRecords", String, /*BuiltInValueConverter.Int32*/null, AllowedMemberLocations.Attribute), true);
        private static Lazy<XamlDirective> s_base = new Lazy<XamlDirective>(() => GetXmlDirective("base"));
        private static Lazy<XamlType> s_boolean = new Lazy<XamlType>(() => GetXamlType(typeof(bool)));
        private static Lazy<XamlType> s_byte = new Lazy<XamlType>(() => GetXamlType(typeof(byte)), true);
        private static Lazy<XamlType> s_char = new Lazy<XamlType>(() => GetXamlType(typeof(char)), true);
        private static Lazy<XamlDirective> s_class = new Lazy<XamlDirective>(() => GetXamlDirective("Class"));
        private static Lazy<XamlDirective> s_classAttributes = new Lazy<XamlDirective>(() => GetXamlDirective("ClassAttributes", s_listOfAttributes.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_classModifier = new Lazy<XamlDirective>(() => GetXamlDirective("ClassModifier"));
        private static Lazy<XamlDirective> s_code = new Lazy<XamlDirective>(() => GetXamlDirective("Code"));
        private static Lazy<XamlDirective> s_connectionId = new Lazy<XamlDirective>(() => GetXamlDirective("ConnectionId", s_string.Value, /*BuiltInValueConverter.Int32*/null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_decimal = new Lazy<XamlType>(() => GetXamlType(typeof(decimal)), true);
        private static Lazy<XamlType> s_double = new Lazy<XamlType>(() => GetXamlType(typeof(double)));
        private static Lazy<XamlDirective> s_factoryMethod = new Lazy<XamlDirective>(() => GetXamlDirective("FactoryMethod", s_string.Value, /*BuiltInValueConverter.String*/null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_fieldModifier = new Lazy<XamlDirective>(() => GetXamlDirective("FieldModifier"));
        private static Lazy<XamlType> s_iNameScope = new Lazy<XamlType>(() => GetXamlType(typeof(INameScope)));
        private static Lazy<XamlDirective> s_initialization = new Lazy<XamlDirective>(() => GetXamlDirective("_Initialization", s_object.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_int16 = new Lazy<XamlType>(() => GetXamlType(typeof(short)), true);
        private static Lazy<XamlType> s_int32 = new Lazy<XamlType>(() => GetXamlType(typeof(int)));
        private static Lazy<XamlType> s_int64 = new Lazy<XamlType>(() => GetXamlType(typeof(long)), true);
        private static Lazy<XamlDirective> s_items = new Lazy<XamlDirective>(() => GetXamlDirective("_Items", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_iXmlSerializable = new Lazy<XamlType>(() => GetXamlType(typeof(IXmlSerializable)), true);
        private static Lazy<XamlDirective> s_key = new Lazy<XamlDirective>(() => GetXamlDirective("Key", s_object.Value, /*BuiltInValueConverter.String*/null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_lang = new Lazy<XamlDirective>(() => GetXmlDirective("lang"));
        private static Lazy<XamlType> s_listOfAttributes = new Lazy<XamlType>(() => GetXamlType(typeof(List<Attribute>)));
        private static Lazy<XamlType> s_listOfMembers = new Lazy<XamlType>(() => GetXamlType(typeof(List<MemberDefinition>)));
        private static Lazy<XamlType> s_listOfObject = new Lazy<XamlType>(() => GetXamlType(typeof(List<object>)));
        private static Lazy<XamlType> s_markupExtension = new Lazy<XamlType>(() => GetXamlType(typeof(MarkupExtension)));
        private static Lazy<XamlType> s_member = new Lazy<XamlType>(() => GetXamlType(typeof(MemberDefinition)));
        private static Lazy<XamlDirective> s_members = new Lazy<XamlDirective>(() => GetXamlDirective("Members", s_listOfMembers.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_name = new Lazy<XamlDirective>(() => GetXamlDirective("Name"));
        private static Lazy<XamlType> s_null = new Lazy<XamlType>(() => GetXamlType(typeof(NullExtension)));
        private static Lazy<XamlType> s_object = new Lazy<XamlType>(() => GetXamlType(typeof(object)));
        private static Lazy<XamlType> s_positionalParameterDescriptor = new Lazy<XamlType>(() => GetXamlType(typeof(PositionalParameterDescriptor)), true);
        private static Lazy<XamlDirective> s_positionalParameters = new Lazy<XamlDirective>(() => GetXamlDirective("_PositionalParameters", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_property = new Lazy<XamlType>(() => GetXamlType(typeof(PropertyDefinition)));
        private static Lazy<XamlType> s_reference = new Lazy<XamlType>(() => GetXamlType(typeof(Reference)));
        private static Lazy<XamlSchemaContext> s_schemaContext = new Lazy<XamlSchemaContext>(new Func<XamlSchemaContext>(XamlLanguage.GetSchemaContext));
        private static Lazy<XamlDirective> s_shared = new Lazy<XamlDirective>(() => GetXamlDirective("Shared"), true);
        private static Lazy<XamlType> s_single = new Lazy<XamlType>(() => GetXamlType(typeof(float)), true);
        private static Lazy<XamlDirective> s_space = new Lazy<XamlDirective>(() => GetXmlDirective("space"));
        private static Lazy<XamlType> s_static = new Lazy<XamlType>(() => GetXamlType(typeof(StaticExtension)));
        private static Lazy<XamlType> s_string = new Lazy<XamlType>(() => GetXamlType(typeof(string)));
        private static Lazy<XamlDirective> s_subclass = new Lazy<XamlDirective>(() => GetXamlDirective("Subclass"), true);
        private static Lazy<XamlDirective> s_synchronousMode = new Lazy<XamlDirective>(() => GetXamlDirective("SynchronousMode"));
        private static Lazy<XamlType> s_timespan = new Lazy<XamlType>(() => GetXamlType(typeof(TimeSpan)), true);
        private static Lazy<XamlType> s_type = new Lazy<XamlType>(() => GetXamlType(typeof(TypeExtension)));
        private static Lazy<XamlDirective> s_typeArguments = new Lazy<XamlDirective>(() => GetXamlDirective("TypeArguments"));
        private static Lazy<XamlDirective> s_uid = new Lazy<XamlDirective>(() => GetXamlDirective("Uid"));
        private static Lazy<XamlDirective> s_unknownContent = new Lazy<XamlDirective>(() => GetXamlDirective("_UnknownContent", AllowedMemberLocations.MemberElement /*, MemberReflector.UnknownReflector*/), true);
        private static Lazy<XamlType> s_uri = new Lazy<XamlType>(() => GetXamlType(typeof(Uri)), true);
        private static ReadOnlyCollection<string> s_xamlNamespaces = new ReadOnlyCollection<string>(new string[] { "http://schemas.microsoft.com/winfx/2006/xaml" });
        private static Lazy<XamlType> s_xDataHolder = new Lazy<XamlType>(() => GetXamlType(typeof(XData)));
        private static ReadOnlyCollection<string> s_xmlNamespaces = new ReadOnlyCollection<string>(new string[] { "http://www.w3.org/XML/1998/namespace" });
        internal const string SWMNamespace = "System.Windows.Markup";
//        private const string x_Arguments = "Arguments";
//        private const string x_AsyncRecords = "AsyncRecords";
//        private const string x_Class = "Class";
//        private const string x_ClassAttributes = "ClassAttributes";
//        private const string x_ClassModifier = "ClassModifier";
//        private const string x_Code = "Code";
//        private const string x_ConnectionId = "ConnectionId";
//        private const string x_FactoryMethod = "FactoryMethod";
//        private const string x_FieldModifier = "FieldModifier";
//        private const string x_Initialization = "_Initialization";
//        private const string x_Items = "_Items";
//        private const string x_Key = "Key";
//        private const string x_Members = "Members";
//        private const string x_Name = "Name";
//        private const string x_PositionalParameters = "_PositionalParameters";
//        private const string x_Shared = "Shared";
//        private const string x_Subclass = "Subclass";
//        private const string x_SynchronousMode = "SynchronousMode";
//        private const string x_TypeArguments = "TypeArguments";
//        private const string x_Uid = "Uid";
//        private const string x_UnknownContent = "_UnknownContent";
        /// <summary>Gets a string value for the string that identifies the XAML (2006) language namespace. That namespace corresponds to the XAML (2006) "x" prefixed namespace as defined in [MS-XAML] Section 5.1.1.</summary>
        public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
//        private const string xml_Base = "base";
//        private const string xml_Lang = "lang";
//        private const string xml_Space = "space";
        /// <summary>Gets a string value for the string that identifies the XML (1998) language namespace. That namespace corresponds to the XML "xml" prefixed namespace as referenced in [MS-XAML] Section 5.1.2.</summary>
        public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";

        private static ReadOnlyCollection<XamlDirective> GetAllDirectives ()
        {
            return new ReadOnlyCollection<XamlDirective>(new XamlDirective[] {
                Arguments, AsyncRecords, Class, Code, ClassModifier, ConnectionId, FactoryMethod, FieldModifier, Key, Initialization, Items, Members, ClassAttributes, Name, PositionalParameters, Shared,
                Subclass, SynchronousMode, TypeArguments, Uid, UnknownContent, Base, Lang, Space
            });
        }

        private static ReadOnlyCollection<XamlType> GetAllTypes ()
        {
            return new ReadOnlyCollection<XamlType>(new XamlType[] {
                Array, Member, Null, Property, Reference, Static, Type, String, Double, Int16, Int32, Int64, Boolean, XData, Object, Char,
                Single, Byte, Decimal, Uri, TimeSpan
            });
        }

        private static XamlSchemaContext GetSchemaContext ()
        {
            Assembly[] referenceAssemblies = new Assembly[] { typeof(System.Xaml.XamlLanguage).Assembly, typeof(MarkupExtension).Assembly };
            XamlSchemaContextSettings settings = new XamlSchemaContextSettings {
                SupportMarkupExtensionsWithDuplicateArity = true
            };
            return new XamlSchemaContext(referenceAssemblies, settings);
        }

        private static XamlDirective GetXamlDirective (string name)
        {
            return GetXamlDirective(name, String, /*BuiltInValueConverter.String*/null, AllowedMemberLocations.Attribute);
        }

        private static XamlDirective GetXamlDirective (string name, AllowedMemberLocations allowedLocation /*, MemberReflector reflector*/)
        {
            return new XamlDirective(s_xamlNamespaces, name, /*allowedLocation, reflector*/null, null, allowedLocation);
        }

        private static XamlDirective GetXamlDirective (string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
        {
            return new XamlDirective(s_xamlNamespaces, name, xamlType, typeConverter, allowedLocation);
        }

        private static XamlType GetXamlType (Type type)
        {
            return s_schemaContext.Value.GetXamlType(type);
        }

        private static XamlDirective GetXmlDirective (string name)
        {
            return new XamlDirective(s_xmlNamespaces, name, String, /*BuiltInValueConverter.String*/null, AllowedMemberLocations.Attribute);
        }

//        internal static Type LookupClrNamespaceType (AssemblyNamespacePair nsPair, string typeName)
//        {
//            if ((nsPair.ClrNamespace == "System.Windows.Markup") && (nsPair.Assembly == typeof(XamlLanguage).Assembly)) {
//                switch (typeName) {
//                    case "Member":
//                        return typeof(MemberDefinition);
//
//                    case "Property":
//                        return typeof(PropertyDefinition);
//                }
//            }
//            return null;
//        }

        internal static XamlDirective LookupXamlDirective (string name)
        {
            switch (name) {
                case "AsyncRecords":
                    return AsyncRecords;

                case "Arguments":
                    return Arguments;

                case "Class":
                    return Class;

                case "ClassModifier":
                    return ClassModifier;

                case "Code":
                    return Code;

                case "ConnectionId":
                    return ConnectionId;

                case "FactoryMethod":
                    return FactoryMethod;

                case "FieldModifier":
                    return FieldModifier;

                case "_Initialization":
                    return Initialization;

                case "_Items":
                    return Items;

                case "Key":
                    return Key;

                case "Members":
                    return Members;

                case "ClassAttributes":
                    return ClassAttributes;

                case "Name":
                    return Name;

                case "_PositionalParameters":
                    return PositionalParameters;

                case "Shared":
                    return Shared;

                case "Subclass":
                    return Subclass;

                case "SynchronousMode":
                    return SynchronousMode;

                case "TypeArguments":
                    return TypeArguments;

                case "Uid":
                    return Uid;

                case "_UnknownContent":
                    return UnknownContent;
            }
            return null;
        }

        internal static XamlType LookupXamlType (string typeNamespace, string typeName)
        {
            if (XamlNamespaces.Contains(typeNamespace)) {
                switch (typeName) {
                    case "Array":
                    case "ArrayExtension":
                        return Array;

                    case "Member":
                        return Member;

                    case "Null":
                    case "NullExtension":
                        return Null;

                    case "Property":
                        return Property;

                    case "Reference":
                    case "ReferenceExtension":
                        return Reference;

                    case "Static":
                    case "StaticExtension":
                        return Static;

                    case "Type":
                    case "TypeExtension":
                        return Type;

                    case "String":
                        return String;

                    case "Double":
                        return Double;

                    case "Int16":
                        return Int16;

                    case "Int32":
                        return Int32;

                    case "Int64":
                        return Int64;

                    case "Boolean":
                        return Boolean;

                    case "XData":
                        return XData;

                    case "Object":
                        return Object;

                    case "Char":
                        return Char;

                    case "Single":
                        return Single;

                    case "Byte":
                        return Byte;

                    case "Decimal":
                        return Decimal;

                    case "Uri":
                        return Uri;

                    case "TimeSpan":
                        return TimeSpan;
                }
            }
            return null;
        }

        internal static XamlDirective LookupXmlDirective (string name)
        {
            switch (name) {
                case "base":
                    return Base;

                case "lang":
                    return Lang;

                case "space":
                    return Space;
            }
            return null;
        }

        internal static string TypeAlias (Type type)
        {
            if (type.Equals(typeof(MemberDefinition))) {
                return "Member";
            }
            if (type.Equals(typeof(PropertyDefinition))) {
                return "Property";
            }
            return null;
        }

        /// <summary>Gets a read-only generic collection of each <see cref="T:System.Xaml.XamlDirective" /> identifier that is defined by .NET Framework XAML Services.</summary>
        /// <returns>A read-only generic collection of each <see cref="T:System.Xaml.XamlDirective" /> identifier that is defined by .NET Framework XAML Services.</returns>
        public static ReadOnlyCollection<XamlDirective> AllDirectives
        {
            get { return s_allDirectives.Value; }
        }

        /// <summary>Gets a read-only generic collection of individual <see cref="T:System.Xaml.XamlType" /> values that match, or alias, a XAML language intrinsic that is defined by .NET Framework XAML Services.</summary>
        /// <returns>A read-only generic collection of each <see cref="T:System.Xaml.XamlType" /> that matches a XAML language intrinsic.</returns>
        public static ReadOnlyCollection<XamlType> AllTypes
        {
            get { return s_allTypes.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Arguments of a factory method or a generic usage.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Arguments of a factory method or generic usage.</returns>
        public static XamlDirective Arguments
        {
            get { return s_arguments.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Array XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Array XAML language intrinsic.</returns>
        public static XamlType Array
        {
            get { return s_array.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the AsyncRecords pseudomember.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the AsyncRecords pseudomember.</returns>
        public static XamlDirective AsyncRecords
        {
            get { return s_asyncRecords.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the base directive from XML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the base directive from XML.</returns>
        public static XamlDirective Base
        {
            get { return s_base.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Boolean XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Boolean XAML language intrinsic.</returns>
        public static XamlType Boolean
        {
            get { return s_boolean.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Byte XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Byte XAML language intrinsic.</returns>
        public static XamlType Byte
        {
            get { return s_byte.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Char XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Char XAML language intrinsic.</returns>
        public static XamlType Char
        {
            get { return s_char.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Class directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Class directive from XAML.</returns>
        public static XamlDirective Class
        {
            get { return s_class.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the ClassAttributes directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the ClassAttributes directive from XAML.</returns>
        public static XamlDirective ClassAttributes
        {
            get { return s_classAttributes.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the ClassModifier directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the ClassModifier directive from XAML.</returns>
        public static XamlDirective ClassModifier
        {
            get { return s_classModifier.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for Code as detailed in [MS-XAML].</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for Code as detailed in [MS-XAML].</returns>
        public static XamlDirective Code
        {
            get { return s_code.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> that identifies a connection point for wiring events to handlers.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> that identifies a connection point for wiring events to handlers.</returns>
        public static XamlDirective ConnectionId
        {
            get { return s_connectionId.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Decimal XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Decimal XAML language intrinsic.</returns>
        public static XamlType Decimal
        {
            get { return s_decimal.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Double XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Double XAML language intrinsic.</returns>
        public static XamlType Double
        {
            get { return s_double.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> that identifies a factory method for XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> that identifies a factory method for XAML.</returns>
        public static XamlDirective FactoryMethod
        {
            get { return s_factoryMethod.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the FieldModifier directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the FieldModifier directive from XAML.</returns>
        public static XamlDirective FieldModifier
        {
            get { return s_fieldModifier.Value; }
        }

        internal static XamlType INameScope
        {
            get { return s_iNameScope.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Initialization directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Initialization directive from XAML.</returns>
        public static XamlDirective Initialization
        {
            get { return s_initialization.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Int16 XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Int16 XAML language intrinsic.</returns>
        public static XamlType Int16
        {
            get { return s_int16.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Int32 XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Int32 XAML language intrinsic.</returns>
        public static XamlType Int32
        {
            get { return s_int32.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Int64 XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Int64 XAML language intrinsic.</returns>
        public static XamlType Int64
        {
            get { return s_int64.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Items directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Items directive from XAML.</returns>
        public static XamlDirective Items
        {
            get { return s_items.Value; }
        }

        internal static XamlType IXmlSerializable
        {
            get { return s_iXmlSerializable.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Key directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Key directive from XAML.</returns>
        public static XamlDirective Key
        {
            get { return s_key.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the lang directive from XML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the lang directive from XML.</returns>
        public static XamlDirective Lang
        {
            get { return s_lang.Value; }
        }

        internal static XamlType MarkupExtension
        {
            get { return s_markupExtension.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the type that is the item type of <see cref="P:System.Xaml.XamlLanguage.Members" />.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the type that is the item type of <see cref="P:System.Xaml.XamlLanguage.Members" />.</returns>
        public static XamlType Member
        {
            get { return s_member.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Members concept in XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Members concept in XAML.</returns>
        public static XamlDirective Members
        {
            get { return s_members.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Name directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Name directive from XAML.</returns>
        public static XamlDirective Name
        {
            get { return s_name.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Null or NullExtension XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Null/NullExtension XAML language intrinsic.</returns>
        public static XamlType Null
        {
            get { return s_null.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Object XAML language concept.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Object XAML language concept.</returns>
        public static XamlType Object
        {
            get { return s_object.Value; }
        }

        internal static XamlType PositionalParameterDescriptor
        {
            get { return s_positionalParameterDescriptor.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the PositionalParameters directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the PositionalParameters directive from XAML.</returns>
        public static XamlDirective PositionalParameters
        {
            get { return s_positionalParameters.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Property concept in XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Property concept in XAML.</returns>
        public static XamlType Property
        {
            get { return s_property.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> that represents a Reference for XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> that represents a Reference for XAML.</returns>
        public static XamlType Reference
        {
            get { return s_reference.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Shared directive for XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Shared directive for XAML.</returns>
        public static XamlDirective Shared
        {
            get { return s_shared.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Single XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Single XAML language intrinsic.</returns>
        public static XamlType Single
        {
            get { return s_single.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the space directive from XML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the space directive from XML.</returns>
        public static XamlDirective Space
        {
            get { return s_space.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Static/StaticExtension XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Static/StaticExtension XAML language intrinsic.</returns>
        public static XamlType Static
        {
            get { return s_static.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the String XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the String XAML language intrinsic.</returns>
        public static XamlType String
        {
            get { return s_string.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Subclass directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Subclass directive from XAML.</returns>
        public static XamlDirective Subclass
        {
            get { return s_subclass.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> that enables loading XAML asynchronously if the XAML processor supports such a mode.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> that enables loading XAML asynchronously.</returns>
        public static XamlDirective SynchronousMode
        {
            get { return s_synchronousMode.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the TimeSpan concept in XAML language.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the TimeSpan XAML language concept.</returns>
        public static XamlType TimeSpan
        {
            get { return s_timespan.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Type/TypeExtension XAML language intrinsic.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Type/TypeExtension XAML language intrinsic.</returns>
        public static XamlType Type
        {
            get { return s_type.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the TypeArguments directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the TypeArguments directive from XAML.</returns>
        public static XamlDirective TypeArguments
        {
            get { return s_typeArguments.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the Uid directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the Uid directive from XAML.</returns>
        public static XamlDirective Uid
        {
            get { return s_uid.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlDirective" /> for the UnknownContent directive from XAML.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlDirective" /> for the UnknownContent directive from XAML.</returns>
        public static XamlDirective UnknownContent
        {
            get { return s_unknownContent.Value; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the Uri XAML language concept.</summary>
        /// <returns>A <see cref="T:System.Xaml.XamlType" /> for the Uri XAML language concept.</returns>
        public static XamlType Uri
        {
            get { return s_uri.Value; }
        }

        /// <summary>Gets a collection of the namespace identifiers for XAML.</summary>
        /// <returns>A collection of the namespace identifiers for XAML.</returns>
        public static IList<string> XamlNamespaces
        {
            [TargetedPatchingOptOut ("PERF")] get { return s_xamlNamespaces; }
        }

        /// <summary>Gets a <see cref="T:System.Xaml.XamlType" /> for the XAML type that backs an XData block in XAML. </summary>
        /// <returns>The <see cref="T:System.Xaml.XamlType" /> for the XAML type that backs an XData block. See [MS-XAML] Section 5.2.23.</returns>
        public static XamlType XData
        {
            get { return s_xDataHolder.Value; }
        }

        /// <summary>Gets a collection of the namespace identifiers for XML.</summary>
        /// <returns>A collection of the namespace identifiers for XML.</returns>
        public static IList<string> XmlNamespaces
        {
            [TargetedPatchingOptOut ("PERF")] get { return s_xmlNamespaces; }
        }
    }

    internal class PositionalParameterDescriptor
    {
        public PositionalParameterDescriptor (object value, bool wasText)
        {
            this.Value = value;
            this.WasText = wasText;
        }

        public object Value { get; set; }

        public bool WasText { get; set; }
    }
}