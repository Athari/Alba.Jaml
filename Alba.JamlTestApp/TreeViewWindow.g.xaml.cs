// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantCast
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Documents.Serialization;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;
using System.Windows.Markup.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.Media3D.Converters;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Alba.JamlTestApp
{
    public partial class TreeViewWindow
    {
        public static IMultiValueConverter _jaml_TreeViewWindowConverter = new _jaml_TreeViewWindowConverter_Class();
        public static IValueConverter _jaml_TreeViewWindowConverter1 = new _jaml_TreeViewWindowConverter1_Class();
        public static IValueConverter _jaml_TreeViewWindowConverter2 = new _jaml_TreeViewWindowConverter2_Class();
        public static IMultiValueConverter _jaml_TreeViewWindowConverter3 = new _jaml_TreeViewWindowConverter3_Class();
        public static IValueConverter _jaml_TreeViewWindowConverter4 = new _jaml_TreeViewWindowConverter4_Class();
 
        private class _jaml_TreeViewWindowConverter_Class : IMultiValueConverter
        {
            public object Convert (object[] values, Type targetType, object param, CultureInfo culture)
            {
                if (values.Any(v => ReferenceEquals(v, DependencyProperty.UnsetValue)))
                    return DependencyProperty.UnsetValue;
                return (bool)(values[0]) && (bool)(values[1]);
            }

            public object[] ConvertBack (object value, Type[] targetTypes, object param, CultureInfo culture)
            {
                throw new NotSupportedException("Converter supports only one-way binding.");
            }
        }
 
        private class _jaml_TreeViewWindowConverter1_Class : IValueConverter
        {
            public object Convert (object value, Type targetType, object param, CultureInfo culture)
            {
                if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                    return DependencyProperty.UnsetValue;
                return !(bool)value;
            }

            public object ConvertBack (object value, Type targetType, object param, CultureInfo culture)
            {
                throw new NotSupportedException("Converter supports only one-way binding.");
            }
        }
 
        private class _jaml_TreeViewWindowConverter2_Class : IValueConverter
        {
            public object Convert (object value, Type targetType, object param, CultureInfo culture)
            {
                if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                    return DependencyProperty.UnsetValue;
                return !(bool)value;
            }

            public object ConvertBack (object value, Type targetType, object param, CultureInfo culture)
            {
                throw new NotSupportedException("Converter supports only one-way binding.");
            }
        }
 
        private class _jaml_TreeViewWindowConverter3_Class : IMultiValueConverter
        {
            public object Convert (object[] values, Type targetType, object param, CultureInfo culture)
            {
                if (values.Any(v => ReferenceEquals(v, DependencyProperty.UnsetValue)))
                    return DependencyProperty.UnsetValue;
                return (bool)(values[0]) && !(bool)(values[1]);
            }

            public object[] ConvertBack (object value, Type[] targetTypes, object param, CultureInfo culture)
            {
                throw new NotSupportedException("Converter supports only one-way binding.");
            }
        }
 
        private class _jaml_TreeViewWindowConverter4_Class : IValueConverter
        {
            public object Convert (object value, Type targetType, object param, CultureInfo culture)
            {
                if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                    return DependencyProperty.UnsetValue;
                return !(bool)value;
            }

            public object ConvertBack (object value, Type targetType, object param, CultureInfo culture)
            {
                throw new NotSupportedException("Converter supports only one-way binding.");
            }
        }
 
    }
}
