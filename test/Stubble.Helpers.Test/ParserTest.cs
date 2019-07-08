using Stubble.Core.Builders;
using Stubble.Core.Parser;
using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Tokens;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class ParserTest
    {
        public ParserPipeline BuildHelperPipeline()
        {
            var builder = new ParserPipelineBuilder();
            builder.AddBefore<InterpolationTagParser>(new HelperTagParser());
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
            Assert.Equal("MyArgument", helperToken.Args[0]);
        }

        [Theory]
        [InlineData("{{MyHelper \"MyArgument\"}}", "\"MyArgument\"")]
        [InlineData("{{MyHelper \"My Argument\"}}", "\"My Argument\"")]
        public void ItParsesHelpersWithStaticArgument(string data, string expectedArgument)
        {
            var parser = new InstanceMustacheParser();
            var pipeline = BuildHelperPipeline();

            var tokens = parser.Parse(data, pipeline: pipeline);

            Assert.Single(tokens.Children);
            var helperToken = Assert.IsType<HelperToken>(tokens.Children[0]);
            Assert.Equal("MyHelper", helperToken.Name.ToString());
            Assert.Equal(expectedArgument, helperToken.Args[0]);
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
            Assert.Equal("MyArgument1", helperToken.Args[0]);
            Assert.Equal("MyArgument2", helperToken.Args[1]);
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
    }
}
