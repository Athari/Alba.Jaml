using System.Runtime;

// ReSharper disable RedundantThisQualifier
// ReSharper disable FieldCanBeMadeReadOnly.Local
namespace Alba.Jaml.MSInternal
{
    internal class LineInfo
    {
        private int _lineNumber;
        private int _linePosition;

        internal LineInfo (int lineNumber, int linePosition)
        {
            this._lineNumber = lineNumber;
            this._linePosition = linePosition;
        }

        public int LineNumber
        {
            [TargetedPatchingOptOut ("PERF")] get { return this._lineNumber; }
        }

        public int LinePosition
        {
            [TargetedPatchingOptOut ("PERF")] get { return this._linePosition; }
        }
    }
}