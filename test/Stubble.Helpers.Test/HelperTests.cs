﻿using System;
using System.Globalization;
using System.Linq;
using McMaster.Extensions.Xunit;
using Stubble.Core.Builders;
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

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("£10.00, £100.26", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToUseRendererContext()
        {
            var helpers = new Helpers()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", context.RenderSettings.CultureInfo);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new Core.Settings.RenderSettings
            {
                CultureInfo = new CultureInfo("en-GB")
            });

            Assert.Equal("£10.00, £100.26", res);
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

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Count}}: {{FormatCurrency Count}}, {{Count2}}: {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("10: £10.00, 100.26: £100.26", res);
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

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("10: £10.00, 100.26: £100.26", res);
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

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{#List}}{{PrintWithComma .}}{{/List}}";

            var list = Enumerable.Range(1, 10).ToArray();

            var res = builder.Render(tmpl, new { List = list });

            Assert.Equal(string.Join(", ", list), res);
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

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency 10.21}}, {{FormatCurrency Count}}";

            var res = builder.Render(tmpl, new { Count = 100.26m });

            Assert.Equal("£10.21, £100.26", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticAndDynamicParameters()
        {
            var helpers = new Helpers()
                .Register<decimal, decimal>("Multiply", (context, count, multiplier) =>
                {
                    return $"{count * multiplier}";
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Multiply 5 5}}, {{Multiply Count 5}}";

            var res = builder.Render(tmpl, new { Count = 2 });

            Assert.Equal("25, 10", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithSpaces()
        {
            var helpers = new Helpers()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value ""I'm Defaulted""}}";

            var res = builder.Render(tmpl, new { Value = "" });

            Assert.Equal("I'm Defaulted", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithEscapedQuotes()
        {
            var helpers = new Helpers()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value 'I\'m Defaulted'}}";

            var res = builder.Render(tmpl, new { Value = "" });

            Assert.Equal("I'm Defaulted", res);
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

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{MyHelper " + staticValue + " Count }}";

            var res = builder.Render(tmpl, new { Count = 10 });

            Assert.Equal($"<Count#10>", res);
        }

        [Fact]
        public void ItShouldAllowRegisteredHelpersWithoutArguments()
        {
            var helpers = new Helpers()
                .Register("PrintListWithComma", (context) => string.Join(", ", context.Lookup<int[]>("List")));

            var builder = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = builder.Render("List: {{PrintListWithComma}}", new { List = new[] { 1, 2, 3 } });

            Assert.Equal("List: 1, 2, 3", res);
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

            Assert.Equal("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is ''", res);
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

            Assert.Equal("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is 'NICKNAME'", res);
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

            Assert.Equal("Name: John Smith", result);
        }
    }
}
