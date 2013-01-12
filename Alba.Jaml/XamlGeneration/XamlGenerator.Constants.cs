using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Alba.Jaml.MSInternal;

namespace Alba.Jaml.XamlGeneration
{
    public partial class XamlGenerator
    {
        public readonly XNamespace Ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public readonly XNamespace NsX = XamlLanguage.Xaml2006Namespace;

        private const string NsXPrefix = XamlLanguage.PreferredPrefix;
        private const string NsLocalPrefix = "my";
        private const string IndentChars = "    ";
        private const string pnDollar = "$";
        private const string pnContent = "_";
        private static readonly Assembly PresentationCore = typeof(Visibility).Assembly;
        private static readonly Assembly PresentationFramework = typeof(Window).Assembly;
        public static readonly string[] WpfNameSpaces = new[] {
            "System.Windows",
            "System.Windows.Annotations",
            "System.Windows.Annotations.Storage",
            "System.Windows.Automation",
            "System.Windows.Automation.Peers",
            "System.Windows.Controls",
            "System.Windows.Controls.Primitives",
            "System.Windows.Data",
            "System.Windows.Documents",
            "System.Windows.Documents.DocumentStructures",
            "System.Windows.Documents.Serialization",
            "System.Windows.Ink",
            "System.Windows.Input",
            "System.Windows.Interop",
            "System.Windows.Markup",
            "System.Windows.Markup.Localizer",
            "System.Windows.Markup.Primitives",
            "System.Windows.Media",
            "System.Windows.Media.Animation",
            "System.Windows.Media.Composition",
            "System.Windows.Media.Converters",
            "System.Windows.Media.Effects",
            "System.Windows.Media.Imaging",
            "System.Windows.Media.Media3D",
            "System.Windows.Media.Media3D.Converters",
            "System.Windows.Media.TextFormatting",
            "System.Windows.Navigation",
            "System.Windows.Resources",
            "System.Windows.Shapes",
            "System.Windows.Shell",
        };
        // Binding ElementName=controlName        {=ref.controlName.Path}
        // Binding RelativeSource=Self            {=this.Path}
        // Binding RelativeSource=TemplatedParent {=tpl.Path}
        // Binding RelativeSource=AncestorType    {=~TypeName.Path}
        // Binding Source=resource                {=@{static.TypeName.Property}.Path}
        // MultiBinding+Converter                 {=${...} + 42 + ${...}}
        // TemplateBinding                        {=@{@resource}.Path}
        // StaticResource Key                     {@Key}
        // DynamicResource Key                    {@=Key}
        // x:Null                                 {null}
        // x:Type                                 {~TypeName}
        // x:Static                               {static.TypeName.Property}
        private static readonly string[][] MarkupAliases = new[] {
            new[] { "{@=", "{DynamicResource " },
            new[] { "{@", "{StaticResource " },
            new[] { "{=}", "{Binding}" },
            new[] { "{~", "{x:Type " },
            new[] { "{static.", "{x:Static " },
            new[] { "{null}", "{x:Null}" },
        };
        private static readonly Dictionary<Type, Type> DefaultItemTypes = new Dictionary<Type, Type> {
            { typeof(SetterBase), typeof(Setter) },
            { typeof(TriggerBase), typeof(Trigger) },
        };
        // Some classes specify read-only props in DictionaryKeyPropertyAttribute
        private static readonly Dictionary<Type, string> DictionaryKeyProps = new Dictionary<Type, string> {
            { typeof(DataTemplate), "DataType" },
            { typeof(ItemContainerTemplate), "DataType" },
        };
    }
}