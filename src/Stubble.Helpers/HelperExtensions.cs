using Stubble.Core.Builders;
using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Core.Settings;

namespace Stubble.Helpers
{
    public delegate string HelperDelegate(StringRender renderer, HelperContext context);

    public static class HelperExtensions
    {
        public static RendererSettingsBuilder AddHelpers(this RendererSettingsBuilder builder, Helpers helpers)
        {
            var pipelineBuilder = new ParserPipelineBuilder();
            pipelineBuilder.AddBefore<InterpolationTagParser>(new HelperTagParser());
            builder.SetParserPipeline(pipelineBuilder.Build());

            builder.TokenRenderers.Add(new HelperTagRenderer(helpers.HelperMap));

            return builder;
        }
    }
}
