using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Stubble.Core.Imported;
using Stubble.Core.Parser;

namespace Stubble.Helpers
{
    public class LinkedHelperTagParser : HelperTagParser
    {
        private const string _linkedTagSeparator = "|";

        public override bool Match(Processor processor, ref StringSlice slice)
        {
            var tag = ParseHelperToken(processor, ref slice);
            if (tag == null)
            {
                return false;
            }

            if (slice.Match(processor.CurrentTags.EndTag))
            {
                processor.CurrentToken = tag;
            }
            else
            {
                var tokens = new List<HelperToken> { tag };
                while (slice.Match(_linkedTagSeparator))
                {
                    slice.NextChar();
                    tag = ParseHelperToken(processor, ref slice, true);
                    if (tag == null)
                    {
                        return false;
                    }
                    tokens.Add(tag);
                }

                if (!slice.Match(processor.CurrentTags.EndTag))
                {
                    return false;
                }
                processor.CurrentToken = new LinkedHelperTokens(tokens);
            }

            slice.Start += processor.CurrentTags.EndTag.Length;
            processor.HasSeenNonSpaceOnLine = true;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool MatchEndTag(Processor processor, ref StringSlice slice, int offset = 0)
            => slice.Match(processor.CurrentTags.EndTag, offset) || slice.Match(_linkedTagSeparator, offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int StartTagLength(Processor processor, ref StringSlice slice)
            => slice.Match(processor.CurrentTags.StartTag) ? processor.CurrentTags.StartTag.Length : _linkedTagSeparator.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int EndTagLength(Processor processor, ref StringSlice slice)
            => slice.Match(processor.CurrentTags.EndTag) ? processor.CurrentTags.EndTag.Length : _linkedTagSeparator.Length;
    }
}
