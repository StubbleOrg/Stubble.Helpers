using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Stubble.Core.Exceptions;
using Stubble.Core.Imported;
using Stubble.Core.Parser;
using Stubble.Core.Parser.Interfaces;

namespace Stubble.Helpers
{
    public class HelperTagParser : InlineParser
    {
        private readonly ImmutableDictionary<string, HelperRef> _helperMap;

        public HelperTagParser(ImmutableDictionary<string, HelperRef> helperMap)
        {
            _helperMap = helperMap;
        }

        public override bool Match(Processor processor, ref StringSlice slice)
        {
            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }
            var tagStart = slice.Start - processor.CurrentTags.StartTag.Length;
            var escapeResult = true;
            var isTripleMustache = false;
            var index = slice.Start;

            while (slice[index].IsWhitespace())
            {
                index++;
            }

            var match = slice[index];
            if (match == '&')
            {
                escapeResult = false;
                index++;
            }
            else if (match == '{')
            {
                escapeResult = false;
                isTripleMustache = true;
                index++;
            }

            while (slice[index].IsWhitespace())
            {
                index++;
            }

            var endTag = isTripleMustache ? '}' + processor.CurrentTags.EndTag : processor.CurrentTags.EndTag;

            var nameStart = index;

            // Skip whitespace or until end tag
            while (!slice[index].IsWhitespace() && !slice.Match(endTag, index - slice.Start))
            {
                index++;
            }

            var name = slice.ToString(nameStart, index);

            // Skip whitespace or until end tag
            while (slice[index].IsWhitespace() && !slice.Match(endTag, index - slice.Start))
            {
                index++;
            }

            if (!_helperMap.TryGetValue(name, out var helperRef))
            {
                return false;
            }

            int contentEnd;
            var argsList = ImmutableArray<HelperArgument>.Empty;
            if (helperRef.ArgumentTypes.Length > 1)
            {
                var argsStart = index;
                slice.Start = index;

                while (!slice.IsEmpty && !slice.Match(endTag))
                {
                    slice.NextChar();
                }

                var args = new StringSlice(slice.Text, argsStart, slice.Start - 1);
                args.TrimEnd();
                contentEnd = args.End + 1;

                argsList = ParseArguments(new StringSlice(args.Text, args.Start, args.End));
            }
            else
            {
                while (!slice.IsEmpty && !slice.Match(endTag))
                {
                    slice.NextChar();
                }

                contentEnd = slice.Start;
            }

            if (!slice.Match(endTag))
            {
                throw new StubbleException($"Unclosed Tag at {slice.Start.ToString(CultureInfo.InvariantCulture)}");
            }

            var tag = new HelperToken
            {
                TagStartPosition = tagStart,
                ContentStartPosition = nameStart,
                Name = name,
                Args = argsList,
                EscapeResult = escapeResult,
                ContentEndPosition = contentEnd,
                TagEndPosition = slice.Start + endTag.Length,
                IsClosed = true
            };
            slice.Start += endTag.Length;

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
