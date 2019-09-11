using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Stubble.Core.Exceptions;
using Stubble.Core.Imported;
using Stubble.Core.Parser;
using Stubble.Core.Parser.Interfaces;

namespace Stubble.Helpers
{
    public class HelperTagParser : InlineParser
    {
        public override bool Match(Processor processor, ref StringSlice slice)
        {
            var tagStart = slice.Start - processor.CurrentTags.StartTag.Length;
            var index = slice.Start;

            while (slice[index].IsWhitespace())
            {
                index++;
            }

            var nameStart = index;

            // Skip whitespace or until end tag
            while (!slice[index].IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                index++;
            }

            var name = slice.ToString(nameStart, index);

            // Skip whitespace or until end tag
            while (slice[index].IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                index++;
            }

            // If we're at an end tag then it's not a helper
            if (slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                return false;
            }

            var argsStart = index;
            slice.Start = index;

            while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
            {
                slice.NextChar();
            }

            var args = new StringSlice(slice.Text, argsStart, slice.Start - 1);
            args.TrimEnd();
            var contentEnd = args.End + 1;

            if (!slice.Match(processor.CurrentTags.EndTag))
            {
                throw new StubbleException($"Unclosed Tag at {slice.Start.ToString()}");
            }

            var argsList = ParseArguments(new StringSlice(args.Text, args.Start, args.End));

            var tag = new HelperToken
            {
                TagStartPosition = tagStart,
                ContentStartPosition = nameStart,
                Name = name,
                Args = argsList,
                ContentEndPosition = contentEnd,
                TagEndPosition = slice.Start + processor.CurrentTags.EndTag.Length,
                IsClosed = true
            };
            slice.Start += processor.CurrentTags.EndTag.Length;

            processor.CurrentToken = tag;
            processor.HasSeenNonSpaceOnLine = true;

            return true;
        }

        private ImmutableArray<HelperArgument> ParseArguments(StringSlice slice)
        {
            slice.TrimStart();
            var args = ImmutableArray.CreateBuilder<HelperArgument>();

            while (!slice.IsEmpty)
            {
                if (slice.CurrentChar == '"' || slice.CurrentChar == '\'')
                {
                    args.Add(ParseQuotedString(ref slice));
                    continue;
                }
                else
                {
                    while (slice.CurrentChar.IsWhitespace())
                    {
                        slice.NextChar();
                    }

                    var start = slice.Start;

                    while (!slice.CurrentChar.IsWhitespace() && !slice.IsEmpty)
                    {
                        slice.NextChar();
                    }

                    args.Add(new HelperArgument(slice.Text.Substring(start, slice.Start - start)));
                }

                while (slice.CurrentChar.IsWhitespace())
                {
                    slice.NextChar();
                }
            }

            return args.ToImmutable();
        }

        private static HelperArgument ParseQuotedString(ref StringSlice slice)
        {
            var startQuote = slice.CurrentChar;
            slice.NextChar();
            var st = slice.Start;

            while (!(slice.CurrentChar == startQuote && slice[slice.Start - 1] != '\\'))
            {
                if (slice.IsEmpty)
                    throw new StubbleException($"Unclosed string at {slice.Start}");

                slice.NextChar();
            }

            var end = slice.Start;
            slice.NextChar();

            return new HelperArgument(Regex.Unescape(slice.Text.Substring(st, end - st)), false);
        }
    }
}
