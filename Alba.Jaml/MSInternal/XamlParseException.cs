using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Xaml;

namespace Alba.Jaml.MSInternal
{
    [Serializable]
    public class XamlParseException : XamlException
    {
        [TargetedPatchingOptOut ("PERF")]
        public XamlParseException ()
        {}

        [TargetedPatchingOptOut ("PERF")]
        public XamlParseException (string message)
            : base(message)
        {}

        internal XamlParseException (MeScanner meScanner, string message)
            : base(message, null, meScanner.LineNumber, meScanner.LinePosition)
        {}

//        internal XamlParseException (XamlScanner xamlScanner, string message)
//            : base(message, null, xamlScanner.LineNumber, xamlScanner.LinePosition)
//        {}

        [TargetedPatchingOptOut ("PERF")]
        protected XamlParseException (SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        [TargetedPatchingOptOut ("PERF")]
        public XamlParseException (string message, Exception innerException)
            : base(message, innerException)
        {}

        internal XamlParseException (int lineNumber, int linePosition, string message)
            : base(message, null, lineNumber, linePosition)
        {}
    }
}