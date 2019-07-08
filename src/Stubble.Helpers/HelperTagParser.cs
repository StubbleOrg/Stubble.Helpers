using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            args.Trim();
            var contentEnd = args.End + 1;

            var tag = new HelperToken
            {
                TagStartPosition = tagStart,
                ContentStartPosition = nameStart,
                Name = name,
                IsClosed = true
            };

            if (!slice.Match(processor.CurrentTags.EndTag))
            {
                throw new StubbleException($"Unclosed Tag at {slice.Start.ToString()}");
            }

            var argsList = new List<string>();
            index = args.Start;
            var argStart = args.Start;
            bool insideString = false;
            while (index <= args.End)
            {
                if (args[index] == '"')
                {
                    if (insideString)
                    {
                        insideString = false;
                    }
                    else
                    {
                        insideString = true;
                    }
                }
                else if (!insideString && (args[index].IsWhitespace() || slice.Match(processor.CurrentTags.EndTag, index)))
                {
                    argsList.Add(args.ToString(argStart, index).Trim());
                    argStart += index - args.Start;
                }
                index++;
            }
            argsList.Add(args.ToString(argStart, index).Trim());

            tag.Args = argsList.ToArray();
            tag.ContentEndPosition = contentEnd;
            tag.TagEndPosition = slice.Start + processor.CurrentTags.EndTag.Length;
            slice.Start += processor.CurrentTags.EndTag.Length;

            processor.CurrentToken = tag;
            processor.HasSeenNonSpaceOnLine = true;

            return true;
        }
    }
}
