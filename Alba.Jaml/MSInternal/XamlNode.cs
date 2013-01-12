using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xaml;

// ReSharper disable RedundantThisQualifier
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantToStringCall
// ReSharper disable SuggestUseVarKeywordEvident
namespace Alba.Jaml.MSInternal
{
    [StructLayout (LayoutKind.Sequential), DebuggerDisplay ("{ToString()}")]
    internal struct XamlNode
    {
        private XamlNodeType _nodeType;
        private InternalNodeType _internalNodeType;
        private object _data;

        public override string ToString ()
        {
            string str = string.Format(CultureInfo.InvariantCulture, "{0}: ", new object[] { this.NodeType });
            switch (this.NodeType) {
                case XamlNodeType.None:
                    switch (this._internalNodeType) {
                        case InternalNodeType.StartOfStream:
                            return (str + "Start Of Stream");

                        case InternalNodeType.EndOfStream:
                            return (str + "End Of Stream");

                        case InternalNodeType.EndOfAttributes:
                            return (str + "End Of Attributes");

                        case InternalNodeType.LineInfo:
                            return (str + "LineInfo: " + this.LineInfo.ToString());
                    }
                    return str;

                case XamlNodeType.StartObject:
                    return (str + this.XamlType.Name);

                case XamlNodeType.GetObject:
                case XamlNodeType.EndObject:
                case XamlNodeType.EndMember:
                    return str;

                case XamlNodeType.StartMember:
                    return (str + this.Member.Name);

                case XamlNodeType.Value:
                    return (str + this.Value.ToString());

                case XamlNodeType.NamespaceDeclaration:
                    return (str + this.NamespaceDeclaration.ToString());
            }
            return str;
        }

        public XamlNodeType NodeType
        {
            get { return this._nodeType; }
        }

        public XamlNode (XamlNodeType nodeType)
        {
            this._nodeType = nodeType;
            this._internalNodeType = InternalNodeType.None;
            this._data = null;
        }

        public XamlNode (XamlNodeType nodeType, object data)
        {
            this._nodeType = nodeType;
            this._internalNodeType = InternalNodeType.None;
            this._data = data;
        }

        public XamlNode (InternalNodeType internalNodeType)
        {
            this._nodeType = XamlNodeType.None;
            this._internalNodeType = internalNodeType;
            this._data = null;
        }

        public XamlNode (LineInfo lineInfo)
        {
            this._nodeType = XamlNodeType.None;
            this._internalNodeType = InternalNodeType.LineInfo;
            this._data = lineInfo;
        }

        public NamespaceDeclaration NamespaceDeclaration
        {
            get
            {
                if (this.NodeType == XamlNodeType.NamespaceDeclaration) {
                    return (NamespaceDeclaration)this._data;
                }
                return null;
            }
        }
        public XamlType XamlType
        {
            get
            {
                if (this.NodeType == XamlNodeType.StartObject) {
                    return (XamlType)this._data;
                }
                return null;
            }
        }
        public object Value
        {
            get
            {
                if (this.NodeType == XamlNodeType.Value) {
                    return this._data;
                }
                return null;
            }
        }
        public XamlMember Member
        {
            get
            {
                if (this.NodeType == XamlNodeType.StartMember) {
                    return (XamlMember)this._data;
                }
                return null;
            }
        }
        public LineInfo LineInfo
        {
            get
            {
                if (this.NodeType == XamlNodeType.None) {
                    return (this._data as LineInfo);
                }
                return null;
            }
        }
        internal bool IsEof
        {
            get { return ((this.NodeType == XamlNodeType.None) && (this._internalNodeType == InternalNodeType.EndOfStream)); }
        }
        internal bool IsEndOfAttributes
        {
            get { return ((this.NodeType == XamlNodeType.None) && (this._internalNodeType == InternalNodeType.EndOfAttributes)); }
        }
        internal bool IsLineInfo
        {
            get { return ((this.NodeType == XamlNodeType.None) && (this._internalNodeType == InternalNodeType.LineInfo)); }
        }

        internal static bool IsEof_Helper (XamlNodeType nodeType, object data)
        {
            if ((nodeType == XamlNodeType.None) && (data is InternalNodeType)) {
                InternalNodeType type = (InternalNodeType)data;
                if (type == InternalNodeType.EndOfStream) {
                    return true;
                }
            }
            return false;
        }

        internal enum InternalNodeType : byte
        {
            EndOfAttributes = 3,
            EndOfStream = 2,
            LineInfo = 4,
            None = 0,
            StartOfStream = 1
        }
    }
}