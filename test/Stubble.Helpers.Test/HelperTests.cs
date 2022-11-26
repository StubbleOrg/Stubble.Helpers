using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using McMaster.Extensions.Xunit;
using Stubble.Core.Builders;
using Stubble.Core.Settings;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class HelperTests
    {
        [Fact]
        [UseCulture("en-GB")]
        public void RegisteredHelpersShouldBeRun()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new RenderSettings
            {
                SkipHtmlEncoding = true,
            });

            res.Should().Be("£10.00, £100.26");
        }

        [Fact]
        public void HelpersShouldBeAbleToUseRendererContext()
        {
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", context.RenderSettings.CultureInfo);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new RenderSettings
            {
                CultureInfo = new CultureInfo("en-GB"),
                SkipHtmlEncoding = true,
            });

            res.Should().Be("£10.00, £100.26");
        }

        [Fact]
        [UseCulture("en-GB")]
        public void StubbleShouldContinueWorkingAsNormal()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Count}}: {{FormatCurrency Count}}, {{Count2}}: {{FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new RenderSettings
            {
                SkipHtmlEncoding = true,
            });

            res.Should().Be("10: £10.00, 100.26: £100.26");
        }

        [Fact]
        [UseCulture("en-GB")]
        public void StubbleShouldContinueWorkingAsNormalWithWhitespace()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{  Count  }}: {{  FormatCurrency Count  }}, {{  Count2  }}: {{  FormatCurrency Count2  }}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new RenderSettings
            {
                SkipHtmlEncoding = true,
            });

            res.Should().Be("10: £10.00, 100.26: £100.26");
        }

        [Fact]
        public void HelpersShouldBeAbleToUseContextLookup()
        {
            var helpers = new Helpers()
                .Register<int>("PrintWithComma", (context, count) =>
                {
                    var arr = context.Lookup<int[]>("List");
                    var index = Array.IndexOf(arr, count);
                    var comma = index != arr.Length - 1
                        ? ", "
                        : "";

                    return $"{count}{comma}";
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{#List}}{{PrintWithComma .}}{{/List}}";

            var list = Enumerable.Range(1, 10).ToArray();

            var res = renderer.Render(tmpl, new { List = list });

            res.Should().Be(string.Join(", ", list));
        }

        [Fact]
        [UseCulture("en-GB")]
        public void HelpersShouldBeAbleToHaveOnlyStaticParameters()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency 10.21}}, {{FormatCurrency Count}}";

            var res = renderer.Render(tmpl, new { Count = 100.26m }, new RenderSettings
            {
                SkipHtmlEncoding = true,
            });

            res.Should().Be("£10.21, £100.26");
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticAndDynamicParameters()
        {
            var helpers = new Helpers()
                .Register<decimal, decimal>("Multiply", (context, count, multiplier) =>
                {
                    return $"{count * multiplier}";
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Multiply 5 5}}, {{Multiply Count 5}}";

            var res = renderer.Render(tmpl, new { Count = 2 });

            res.Should().Be("25, 10");
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithSpaces()
        {
            var helpers = new Helpers()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value ""I'm Defaulted""}}";

            var res = renderer.Render(tmpl, new { Value = "" });

            res.Should().Be("I&#39;m Defaulted");
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithEscapedQuotes()
        {
            var helpers = new Helpers()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value 'I\'m Defaulted'}}";

            var res = renderer.Render(tmpl, new { Value = "" });

            res.Should().Be("I&#39;m Defaulted");
        }

        [Theory]
        [InlineData("'Count'")]
        [InlineData("\"Count\"")]
        public void ItShouldCallHelperWhenExistsStaticAndDynamicVariable(string staticValue)
        {
            var helpers = new Helpers()
                .Register<string, int>("MyHelper", (context, staticVariable, dynamicVariable) =>
                {
                    return $"<{staticVariable}#{dynamicVariable}>";
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{MyHelper " + staticValue + " Count }}";

            var res = renderer.Render(tmpl, new { Count = 10 }, new RenderSettings { SkipHtmlEncoding = true });

            res.Should().Be($"<Count#10>");
        }

        [Fact]
        public void ItShouldAllowRegisteredHelpersWithoutArguments()
        {
            var helpers = new Helpers()
                .Register("PrintListWithComma", (context) => string.Join(", ", context.Lookup<int[]>("List")));

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = renderer.Render("List: {{PrintListWithComma}}", new { List = new[] { 1, 2, 3 } });

            res.Should().Be("List: 1, 2, 3");
        }

        [Fact]
        public void ItShouldNotRenderHelperWithMissingLookedUpArgumentThatIsntValueType()
        {
            var helpers = new Helpers()
                .Register<string>("ToCapitalLetters", (context, arg) => arg.ToUpperInvariant());

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = renderer.Render("User name is '{{Name}}' and nickname is '{{Nickname}}'. In capital letters name is '{{ToCapitalLetters Name}}' and nickname is '{{ToCapitalLetters Nickname}}'", new
            {
                Name = "John"
            });

            res.Should().Be("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is ''");
        }

        [Fact]
        public void ItShouldRenderHelperWithConstantQuotedStringArgument()
        {
            var helpers = new Helpers()
                .Register<string>("ToCapitalLetters", (context, arg) => arg.ToUpperInvariant());

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = renderer.Render("User name is '{{Name}}' and nickname is '{{Nickname}}'. In capital letters name is '{{ToCapitalLetters Name}}' and nickname is '{{ToCapitalLetters 'Nickname'}}'", new
            {
                Name = "John"
            });

            res.Should().Be("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is 'NICKNAME'");
        }

        [Fact]
        public void ItShouldRenderHelperWithTwoConstantArguments()
        {
            var helpers = new Helpers()
                .Register("ReplaceString", (HelperContext context, string searchString, string oldString, string newString) => searchString?.Replace(oldString, newString, StringComparison.InvariantCulture));

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var result = renderer.Render("Name: {{ReplaceString Name 'XXX' ' '}}", new
            {
                Name = "JohnXXXSmith"
            });

            result.Should().Be("Name: John Smith");
        }

        [Fact]
        public void ItShouldRenderByDefaultEscaped()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            res.Should().Be("&#163;10.00, &#163;100.26");
        }

        [Fact]
        public void ItShouldSkipHtmlEncodingWhenSet()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new RenderSettings
            {
                SkipHtmlEncoding = true,
            });

            res.Should().Be("£10.00, £100.26");
        }

        [Fact]
        public void ItShouldRenderWithEscapedContextTripleBrackets()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{{FormatCurrency Count}}}, {{{FormatCurrency Count2}}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            res.Should().Be("£10.00, £100.26");
        }

        [Fact]
        public void ItShouldRenderWithAmpersandEscapedContext()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var renderer = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{& FormatCurrency Count}}, {{& FormatCurrency Count2}}";

            var res = renderer.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            res.Should().Be("£10.00, £100.26");
        }
    }
}
