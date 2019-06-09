# Stubble Extensions - Helpers ![AppVeyor branch](https://img.shields.io/appveyor/ci/Romanx/stubble-helpers/master.svg?label=Appveyor%20Build&style=flat-square)![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Stubble.Helpers.svg?label=Nuget%20Pre-Release&style=flat-square)
_Currently In Progress_

Stubble helpers is an opinionated method of registering helpers with the Stubble renderer to call certain methods with arguments while rendering your templates much like [Handlebars.js helpers](https://handlebarsjs.com/expressions.html);

To get started with helpers, include the package from nuget and register your helpers like so.
```csharp
var helpers = new Helpers()
    .Register"FormatCurrency", (HelperContext context, decimal count) => count.ToString("C", culture));

var stubble = new StubbleBuilder()
    .Configure(conf => conf.AddHelpers(helpers))
    .Build();

var result = stubble.Render("{{FormatCurrency Count}}", new { Count = 100.26m });

Assert.Equal("£100.26", result);
```

For more advanced cases you can use the `HelperContext` to get access to values in your current context in a strongly typed manner like the following:
```csharp
var helpers = new Helpers()
    .Register("PrintListWithComma", (context) => string.Join(", ", context.Lookup<int[]>("List")));

var builder = new StubbleBuilder()
    .Configure(conf => conf.AddHelpers(helpers))
    .Build();

var res = builder.Render("List: {{PrintListWithComma}}", new { List = new[] { 1, 2, 3 } });

Assert.Equal("1, 2, 3", res);
```

## Limitations
This uses the C# `Func` delegates for registering these functions and so you're limited to 15 parameters but we feel this is a pretty fair limitation and anything more and you should be preprocessing your data before rendering.

There is also no async support inside your helpers for the same reasons since you should be preprocessing your data before rendering in this case.