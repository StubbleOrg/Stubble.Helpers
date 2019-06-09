using System;
using System.Globalization;
using System.Linq;
using Stubble.Core.Builders;
using Stubble.Helpers;
using Xunit;

public class HelperTests
{
    [Fact]
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
}
