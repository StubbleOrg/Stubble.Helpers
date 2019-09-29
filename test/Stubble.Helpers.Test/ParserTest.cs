using System.Collections.Immutable;
using Stubble.Core.Builders;
using Stubble.Core.Parser;
using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Tokens;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class ParserTest
    {
        public ParserPipeline BuildHelperPipeline(Helpers helpers)
        {
            var builder = new ParserPipelineBuilder();
            builder.AddBefore<InterpolationTagParser>(new HelperTagParser(helpers.HelperMap));
            return builder.Build();
        }

        [Fact]
        public void ItDoesntParseUnregisteredHelpers()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register("MyHelper", ctx => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper2}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            Assert.IsType<InterpolationToken>(tokens.Children[0]);
        }

        [Fact]
        public void ItParsesHelpersWithoutArguments()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register("MyHelper", ctx => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            Assert.IsType<HelperToken>(tokens.Children[0]);
        }

        [Fact]
        public void ItParsesHelpersWithArgument()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<int>("MyHelper", (ctx, arg) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

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
            var helpers = new Helpers()
                .Register<int, int>("MyHelper", (ctx, arg1, arg2) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

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
            var pipeline = BuildHelperPipeline(new Helpers());

            var tokens = parser.Parse("{{#MyHelper}}{{/MyHelper}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            Assert.IsType<SectionToken>(tokens.Children[0]);
        }

        [Fact]
        public void ItShouldBeAbleToParseStaticParameters()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<int, int>("MyHelper", (ctx, arg1, arg2) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

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
            var helpers = new Helpers()
                .Register<string>("MyHelper", (ctx, arg) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

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
            var helpers = new Helpers()
                .Register<string>("MyHelper", (ctx, arg) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse($"{{{{MyHelper {argument}}}}}", pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal(argumentValue, helperToken.Args[0].Value);
            Assert.False(helperToken.Args[0].ShouldAttemptContextLoad);
        }
    }
}
