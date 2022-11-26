using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Core.Settings;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class RendererTests
    {
        [Fact]
        public void ItShouldRenderNothingWhenHelperDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.Create<string, HelperRef>();

            var tagRenderer = new HelperTagRenderer(helpers);

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().BeEmpty();
        }

        [Fact]
        public void ItShouldCallHelperWhenExists()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<10>");
        }

        [Fact]
        public void ItShouldCallHelperWhenExistsWithArgumentFromParent()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, int, string>((helperContext, count, count2) =>
            {
                return $"<{count}-{count2}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"), new HelperArgument("Count2"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings)
                .Push(new { Count2 = 20 });

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<10-20>");
        }

        [Fact]
        public void ItShouldCallHelperWhenExistsWithArgumentOverrideFromParent()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, int, string>((helperContext, count, count2) =>
            {
                return $"<{count}-{count2}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"), new HelperArgument("Count2"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings)
                .Push(new { Count = 20, Count2 = 20 });

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<20-20>");
        }

        [Fact]
        public void ItShouldRenderNothingWhenValueDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count1"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().BeEmpty();
        }

        [Fact]
        public void ItShouldRenderNothingWhenTypesDoNotMatch()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = "wrong-type" }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().BeEmpty();
        }

        [Fact]
        public void ItShouldRenderWhenTypesNotMatchCanBeConverted()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = "10" }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<10>");
        }

        [Fact]
        public void ItShouldRenderWhenTypesMatchBaseType()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, object, string>((helperContext, src) =>
            {
                if (!(src is IDictionary<object, object> dic))
                {
                    return string.Empty;
                }
                return $"<{dic["value"]}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = new Dictionary<object, object> { { "value", "10" } } }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<10>");
        }

        [Fact]
        public void ItShouldRenderAllowHelpersWithNoArguments()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, string>((helperContext) =>
            {
                return $"<{helperContext.Lookup<int>("Count")}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("<10>");
        }

        [Fact]
        public void ItShouldApplyTheCultureOfTheRenderer()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings
            {
                CultureInfo = new CultureInfo("ru-RU")
            };
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, decimal>((helperContext) =>
            {
                return helperContext.Lookup<decimal>("Count");
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 1.21m }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().Be("1,21");
        }

        [Fact]
        public void ItShouldHandleNullReturnFromHelper()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, object>((helperContext) =>
            {
                return null;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTagRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 1.21m }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            res.Should().BeEmpty();
        }
    }
}
