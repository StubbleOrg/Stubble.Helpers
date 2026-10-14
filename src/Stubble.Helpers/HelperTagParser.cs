using System.Collections.Immutable;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
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
            var originalSliceStart = slice.Start;
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
            var bodyStart = index;
            while (!slice.IsEmpty && !slice.Match(endTag))
            {
                slice.NextChar();
            }

            var bodyEnd = slice.Start;
            if (!slice.Match(endTag))
            {
                throw new StubbleException($"Unclosed Tag at {slice.Start.ToString(CultureInfo.InvariantCulture)}");
            }

            var stages = SplitStages(slice.ToString(bodyStart, bodyEnd));
            var firstStage = stages[0].Trim();
            var firstName = ReadName(firstStage, out var firstArgsText);
            var pipeline = ImmutableArray.CreateBuilder<HelperStage>();
            var argsList = ImmutableArray<HelperArgument>.Empty;
            HelperArgument? initialValue = null;
            string name;

            if (firstName.Length > 0 && _helperMap.TryGetValue(firstName, out var helperRef))
            {
                name = firstName;
                if (helperRef.ArgumentTypes.Length > 1)
                {
                    argsList = ParseArguments(firstArgsText);
                }
            }
            else if (stages.Count > 1)
            {
                var initialValues = ParseArguments(firstStage);
                if (initialValues.Length != 1)
                {
                    throw new StubbleException("A helper pipeline must start with one value or a registered helper.");
                }

                name = null;
                initialValue = initialValues[0];
            }
            else
            {
                slice.Start = originalSliceStart;
                return false;
            }

            for (var stageIndex = 1; stageIndex < stages.Count; stageIndex++)
            {
                var stageText = stages[stageIndex].Trim();
                var stageName = ReadName(stageText, out var stageArgsText);
                if (stageName.Length == 0 || !_helperMap.TryGetValue(stageName, out var stageHelper))
                {
                    throw new StubbleException($"Unknown helper in pipeline stage '{stageName}'.");
                }

                var stageArgs = ParseArguments(stageArgsText);
                if (stageHelper.ArgumentTypes.Length == 1)
                {
                    stageArgs = ImmutableArray<HelperArgument>.Empty;
                }

                pipeline.Add(new HelperStage(stageName, stageArgs));
            }

            var tag = new HelperToken
            {
                TagStartPosition = tagStart,
                ContentStartPosition = nameStart,
                Name = name,
                Args = argsList,
                InitialValue = initialValue,
                Pipeline = pipeline.ToImmutable(),
                EscapeResult = escapeResult,
                ContentEndPosition = bodyEnd,
                TagEndPosition = slice.Start + endTag.Length,
                IsClosed = true
            };
            slice.Start += endTag.Length;

            processor.CurrentToken = tag;
            processor.HasSeenNonSpaceOnLine = true;

            return true;
        }

        private static List<string> SplitStages(string input)
        {
            var stages = new List<string>();
            var stageStart = 0;
            var quote = '\0';

            for (var index = 0; index < input.Length; index++)
            {
                var current = input[index];
                if (quote != '\0')
                {
                    if (current == '\\' && index + 1 < input.Length)
                    {
                        index++;
                    }
                    else if (current == quote)
                    {
                        quote = '\0';
                    }
                }
                else if (current == '"' || current == '\'')
                {
                    quote = current;
                }
                else if (current == '|')
                {
                    stages.Add(input.Substring(stageStart, index - stageStart));
                    stageStart = index + 1;
                }
            }

            if (quote != '\0')
            {
                throw new StubbleException("Unclosed string in helper tag.");
            }

            stages.Add(input.Substring(stageStart));
            return stages;
        }

        private static string ReadName(string stage, out string args)
        {
            var start = 0;
            while (start < stage.Length && char.IsWhiteSpace(stage[start]))
            {
                start++;
            }

            var end = start;
            while (end < stage.Length && !char.IsWhiteSpace(stage[end]))
            {
                end++;
            }

            args = stage.Substring(end);
            return stage.Substring(start, end - start);
        }

        private static ImmutableArray<HelperArgument> ParseArguments(string input)
        {
            var args = ImmutableArray.CreateBuilder<HelperArgument>();
            var index = 0;
            while (index < input.Length)
            {
                while (index < input.Length && char.IsWhiteSpace(input[index]))
                {
                    index++;
                }

                if (index >= input.Length)
                {
                    break;
                }

                if (input[index] == '"' || input[index] == '\'')
                {
                    var quote = input[index++];
                    var valueStart = index;
                    var value = new StringBuilder();
                    var closed = false;
                    while (index < input.Length)
                    {
                        if (input[index] == '\\' && index + 1 < input.Length)
                        {
                            value.Append(input[index++]);
                            value.Append(input[index++]);
                        }
                        else if (input[index] == quote)
                        {
                            closed = true;
                            break;
                        }
                        else
                        {
                            value.Append(input[index++]);
                        }
                    }

                    if (!closed)
                    {
                        throw new StubbleException($"Unclosed string at {valueStart.ToString(CultureInfo.InvariantCulture)}");
                    }

                    args.Add(new HelperArgument(Regex.Unescape(value.ToString()), false));
                    index++;
                }
                else
                {
                    var value = new StringBuilder();
                    while (index < input.Length && !char.IsWhiteSpace(input[index]))
                    {
                        value.Append(input[index++]);
                    }

                    args.Add(new HelperArgument(value.ToString()));
                }
            }

            return args.ToImmutable();
        }
    }
}
