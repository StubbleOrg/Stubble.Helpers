
using System;
using System.Collections.Immutable;
using System.IO;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Core.Settings;
using Stubble.Helpers;
using Xunit;

public class RendererTests 
{
    [Fact]
    public void ItShouldRenderNothingWhenHelperDoesntExist()
    {
        var writer = new StringWriter();
        var settings = new RendererSettingsBuilder().BuildSettings();
        var renderSettings = new RenderSettings();
        var stringRenderer = new StringRender(writer, settings.RendererPipeline);

        var helpers = ImmutableDictionary.Create<string, Delegate>();

        var tagRenderer = new HelperTagRenderer(helpers);

        var token = new HelperToken {
            Name = "MyHelper",
            Args = Array.Empty<string>()
        };

        var context = new Context(new { Count = 10 }, settings, renderSettings);

        tagRenderer.Write(stringRenderer, token, context);

        var res = writer.ToString();

        Assert.Equal("", res);
    }

    [Fact]
    public void ItShouldCallHelperWhenExists()
    {
        var writer = new StringWriter();
        var settings = new RendererSettingsBuilder().BuildSettings();
        var renderSettings = new RenderSettings();
        var stringRenderer = new StringRender(writer, settings.RendererPipeline);

        var helpers = ImmutableDictionary.CreateBuilder<string, Delegate>();

        helpers.Add("MyHelper", new Func<HelperContext, int, string>((helperContext, count) => {
            return $"<{count}>";
        }));

        var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

        var token = new HelperToken {
            Name = "MyHelper",
            Args = new[] { "Count" }
        };

        var context = new Context(new { Count = 10 }, settings, renderSettings);

        tagRenderer.Write(stringRenderer, token, context);

        var res = writer.ToString();

        Assert.Equal("<10>", res);
    }
}
