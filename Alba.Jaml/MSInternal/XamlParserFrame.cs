using System.Runtime;
using System.Xaml;

// ReSharper disable RedundantThisQualifier
namespace Alba.Jaml.MSInternal
{
    internal class XamlParserFrame : XamlCommonFrame
    {
        public override void Reset ()
        {
            base.Reset();
            this.PreviousChildType = null;
            this.CtorArgCount = 0;
            this.ForcedToUseConstructor = false;
            this.InCollectionFromMember = false;
            this.InImplicitArray = false;
            this.InContainerDirective = false;
            this.TypeNamespace = null;
        }

        public int CtorArgCount { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public bool ForcedToUseConstructor { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public bool InCollectionFromMember { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public bool InContainerDirective { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public bool InImplicitArray { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public XamlType PreviousChildType { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }

        public string TypeNamespace { [TargetedPatchingOptOut ("PERF")] get; [TargetedPatchingOptOut ("PERF")] set; }
    }
}