using FluentAssertions;
using FluentAssertions.Execution;
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

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<InterpolationToken>();
        }

        [Fact]
        public void ItParsesHelpersWithoutArguments()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register("MyHelper", ctx => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>();
        }

        [Fact]
        public void ItParsesHelpersWithArgument()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<int>("MyHelper", (ctx, arg) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper MyArgument}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[] { new HelperArgument("MyArgument", true) }
                });
        }

        [Fact]
        public void ItParsesHelpersWithMultipleLookupArguments()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<int, int>("MyHelper", (ctx, arg1, arg2) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper MyArgument1 MyArgument2}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[]
                    {
                            new HelperArgument("MyArgument1", true),
                            new HelperArgument("MyArgument2", true),
                    },
                });
        }

        [Theory]
        [InlineData("\"MyArgument1\"", "MyArgument1", "\"MyArgument2\"", "MyArgument2")]
        [InlineData("\'MyArgument1\'", "MyArgument1", "\'MyArgument2\'", "MyArgument2")]
        [InlineData("\"MyArgument1\"", "MyArgument1", "\'MyArgument2\'", "MyArgument2")]
        [InlineData("\'MyArgument1\'", "MyArgument1", "\"MyArgument2\"", "MyArgument2")]
        public void ItParsesHelpersWithMultipleStaticArguments(string arg1, string arg1Value, string arg2, string arg2Value )
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<string, string>("MyHelper", (ctx, arg1x, arg2x) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse($"{{{{MyHelper {arg1} {arg2}}}}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[]
                    {
                        new HelperArgument(arg1Value, false),
                        new HelperArgument(arg2Value, false),
                    },
                });
        }

        [Fact]
        public void OnlyParsesHelperSectionsWithArguments()
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline(new Helpers());

            var tokens = parser.Parse("{{#MyHelper}}{{/MyHelper}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<SectionToken>();
        }

        [Fact]
        public void ItShouldBeAbleToParseStaticParameters()
        {
            var parser = new InstanceMustacheParser();
            var helpers = new Helpers()
                .Register<int, int>("MyHelper", (ctx, arg1, arg2) => "Foo");
            var pipeline = BuildHelperPipeline(helpers);

            var tokens = parser.Parse("{{MyHelper 10 MyArgument2}}", pipeline: pipeline);

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[]
                    {
                        new HelperArgument("10", true),
                        new HelperArgument("MyArgument2", true),
                    },
                });
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

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[]
                    {
                        new HelperArgument(argumentValue, false),
                    },
                });
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

            tokens.Children
                .Should()
                .ContainSingle()
                .Which.Should().BeOfType<HelperToken>()
                .Which.Should().BeEquivalentTo(new
                {
                    Name = "MyHelper",
                    Args = new[]
                    {
                        new HelperArgument(argumentValue, false),
                    },
                });
        }
    }
}
