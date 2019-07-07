using Stubble.Core.Builders;
using Stubble.Core.Parser;
using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Tokens;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class ParserTest
    {
        public ParserPipeline BuildHelperPipeline(bool allowLinkedParsers = false)
        {
            var builder = new ParserPipelineBuilder();
            builder.AddBefore<InterpolationTagParser>(allowLinkedParsers ? new LinkedHelperTagParser() :  new HelperTagParser());
            return builder.Build();
        }

        [Fact]
        public void OnlyParsesHelpersWithArguments()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse("{{MyHelper}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            Assert.IsType<InterpolationToken>(tokens.Children[0]);
        }

        [Fact]
        public void ItParsesHelpersWithArgument()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse("{{MyHelper MyArgument}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal("MyArgument", helperToken.Args[0].Value);
            Assert.True(helperToken.Args[0].ShouldAttemptContextLoad);
        }

        [Fact]
        public void ItParsesHelpersWithMultipleArguments()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse("{{MyHelper MyArgument1 MyArgument2}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal("MyArgument1", helperToken.Args[0].Value);
            Assert.True(helperToken.Args[0].ShouldAttemptContextLoad);
            Assert.Equal("MyArgument2", helperToken.Args[1].Value);
            Assert.True(helperToken.Args[0].ShouldAttemptContextLoad);
        }

        [Fact]
        public void OnlyParsesHelperSectionsWithArguments()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse("{{#MyHelper}}{{/MyHelper}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            Assert.IsType<SectionToken>(tokens.Children[0]);
        }

        [Fact]
        public void ItShouldBeAbleToParseStaticParameters()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse("{{MyHelper 10 MyArgument2}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal("10", helperToken.Args[0].Value);
            Assert.True(helperToken.Args[0].ShouldAttemptContextLoad);
            Assert.Equal("MyArgument2", helperToken.Args[1].Value);
            Assert.True(helperToken.Args[1].ShouldAttemptContextLoad);
        }

        [Theory]
        [InlineData("\"Quoted\"")]
        [InlineData("\'Quoted\'")]
        public void ItShouldBeAbleToParseStaticParametersWithQuotes(object value)
        {
            var argument = (string)value;
            var argumentValue = argument
                .Replace("\"", string.Empty)
                .Replace("\'", string.Empty);

            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse($"{{{{MyHelper {argument}}}}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal(argumentValue, helperToken.Args[0].Value);
            Assert.False(helperToken.Args[0].ShouldAttemptContextLoad);
        }

        [Theory]
        [InlineData("\"Quoted With Spaces\"")]
        [InlineData("\'Quoted With Spaces\'")]
        public void ItShouldBeAbleToParseStaticParametersWithQuotesAndSpaces(object value)
        {
            var argument = (string)value;
            var argumentValue = argument
                .Replace("\"", string.Empty)
                .Replace("\'", string.Empty);

            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse($"{{{{MyHelper {argument}}}}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal(argumentValue, helperToken.Args[0].Value);
            Assert.False(helperToken.Args[0].ShouldAttemptContextLoad);
        }

        [Theory]
        [InlineData("{{MyHelper MyArgument1|MyHelper2}}")]
        [InlineData("{{ MyHelper MyArgument1 | MyHelper2 }}")]
        public void ItParsesLinkedHelpersWithoutArguments(string data)
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline(true);

            var tokens = parser.Parse(data, pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<LinkedHelperTokens>(tokens.Children[0]);
            Assert.Equal(2, helperToken.Tokens.Count);

            Assert.Equal("MyHelper", helperToken.Tokens[0].Name.ToString());
            Assert.Single(helperToken.Tokens[0].Args);
            Assert.Equal("MyArgument1", helperToken.Tokens[0].Args[0].Value);

            Assert.Equal("MyHelper2", helperToken.Tokens[1].Name.ToString());
            Assert.Empty(helperToken.Tokens[1].Args);
        }

        [Theory]
        [InlineData("{{MyHelper MyArgument1|MyHelper2 MyArgument2}}")]
        [InlineData("{{ MyHelper MyArgument1 | MyHelper2 MyArgument2 }}")]
        public void ItParsesLinkedHelpersWithWithArguments(string data)
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline(true);

            var tokens = parser.Parse(data, pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<LinkedHelperTokens>(tokens.Children[0]);
            Assert.Equal(2, helperToken.Tokens.Count);

            Assert.Equal("MyHelper", helperToken.Tokens[0].Name.ToString());
            Assert.Single(helperToken.Tokens[0].Args);
            Assert.Equal("MyArgument1", helperToken.Tokens[0].Args[0].Value);

            Assert.Equal("MyHelper2", helperToken.Tokens[1].Name.ToString());
            Assert.Single(helperToken.Tokens[1].Args);
            Assert.Equal("MyArgument2", helperToken.Tokens[1].Args[0].Value);
        }

        [Theory]
        [InlineData("{{MyHelper MyArgument1 MyArgument2|MyHelper2 MyArgument3 MyArgument4}}")]
        [InlineData("{{ MyHelper MyArgument1 MyArgument2 | MyHelper2 MyArgument3 MyArgument4 }}")]
        public void ItParsesLinkedHelpersWithMultipleArguments(string data)
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline(true);

            var tokens = parser.Parse(data, pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<LinkedHelperTokens>(tokens.Children[0]);
            Assert.Equal(2, helperToken.Tokens.Count);

            Assert.Equal("MyHelper", helperToken.Tokens[0].Name.ToString());
            Assert.Equal(2, helperToken.Tokens[0].Args.Length);
            Assert.Equal("MyArgument1", helperToken.Tokens[0].Args[0].Value);
            Assert.Equal("MyArgument2", helperToken.Tokens[0].Args[1].Value);

            Assert.Equal("MyHelper2", helperToken.Tokens[1].Name.ToString());
            Assert.Equal(2, helperToken.Tokens[1].Args.Length);
            Assert.Equal("MyArgument3", helperToken.Tokens[1].Args[0].Value);
            Assert.Equal("MyArgument4", helperToken.Tokens[1].Args[1].Value);
        }
    }
}
