using System.Collections.Immutable;
using Stubble.Core.Tokens;

namespace Stubble.Helpers
{
    public class HelperToken : InlineToken<HelperToken>, INonSpace
    {
        public string Name { get; set; }

        public ImmutableArray<HelperArgument> Args { get; set; }

        public bool EscapeResult { get; set; }

        public override bool Equals(HelperToken other) =>
            other is object
            && (other.TagStartPosition, other.TagEndPosition, other.ContentStartPosition, other.ContentEndPosition, other.IsClosed)
                == (TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, IsClosed)
            && other.Content.Equals(Content)
            && other.Name.Equals(Name, System.StringComparison.OrdinalIgnoreCase)
            && CompareHelper.CompareImmutableArraysWithEquatable(Args, other.Args)
            && other.EscapeResult == EscapeResult;

        public override bool Equals(object obj)
            => obj is HelperToken a && Equals(a);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => (Name, Args, TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, Content, IsClosed, EscapeResult).GetHashCode();
    }
}
