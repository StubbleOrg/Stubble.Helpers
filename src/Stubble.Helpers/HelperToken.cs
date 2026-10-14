using System.Collections.Immutable;
using Stubble.Core.Tokens;

namespace Stubble.Helpers
{
    public class HelperToken : InlineToken<HelperToken>, INonSpace
    {
        public string Name { get; set; }

        public ImmutableArray<HelperArgument> Args { get; set; }

        public HelperArgument? InitialValue { get; set; }

        public ImmutableArray<HelperStage> Pipeline { get; set; } = ImmutableArray<HelperStage>.Empty;

        public bool EscapeResult { get; set; }

        public override bool Equals(HelperToken other) =>
            other is object
            && (other.TagStartPosition, other.TagEndPosition, other.ContentStartPosition, other.ContentEndPosition, other.IsClosed)
                == (TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, IsClosed)
            && other.Content.Equals(Content)
            && other.Name.Equals(Name, System.StringComparison.OrdinalIgnoreCase)
            && CompareHelper.CompareImmutableArraysWithEquatable(Args, other.Args)
            && other.InitialValue == InitialValue
            && (Pipeline.IsDefaultOrEmpty && other.Pipeline.IsDefaultOrEmpty
                || CompareHelper.CompareImmutableArraysWithEquatable(Pipeline, other.Pipeline))
            && other.EscapeResult == EscapeResult;

        public override bool Equals(object obj)
            => obj is HelperToken a && Equals(a);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = (Name, Args, InitialValue, TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, Content, IsClosed, EscapeResult).GetHashCode();
            if (!Pipeline.IsDefault)
            {
                foreach (var stage in Pipeline)
                {
                    hashCode = unchecked(hashCode * -1521134295 + stage.GetHashCode());
                }
            }

            return hashCode;
        }
    }

}
