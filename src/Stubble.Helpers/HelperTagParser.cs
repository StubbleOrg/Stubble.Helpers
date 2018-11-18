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

            while (!slice[index].IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                index++;
            }

            var name = slice.ToString(nameStart, index);

            if (!slice[index].IsWhitespace())
            {
                return false;
            }
            index++;

            var argsStart = index;
            slice.Start = index;

            while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
            {
                slice.NextChar();
            }

            var args = new StringSlice(slice.Text, argsStart, slice.Start - 1);
            args.TrimEnd();
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

            var argsList = args.ToString().Split(' ');
            for (var i = 0; i < argsList.Length; i++)
            {
                argsList[i] = argsList[i].Trim();
            }

            tag.Args = argsList;
            tag.ContentEndPosition = contentEnd;
            tag.TagEndPosition = slice.Start + processor.CurrentTags.EndTag.Length;
            slice.Start += processor.CurrentTags.EndTag.Length;

            processor.CurrentToken = tag;
            processor.HasSeenNonSpaceOnLine = true;

            return true;
        }
    }
}
