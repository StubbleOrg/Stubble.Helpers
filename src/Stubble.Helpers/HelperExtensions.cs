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
            builder.ConfigureParserPipeline(pipelineBuilder => pipelineBuilder
                .AddBefore<InterpolationTagParser>(helpers.IsLinkedHelpersAllowed ? new LinkedHelperTagParser() : new HelperTagParser()));

            var helperTagRender = new HelperTagRenderer(helpers.HelperMap);
            builder.TokenRenderers.Add(helperTagRender);
            builder.TokenRenderers.Add(new LinkedHelperRenderer(helperTagRender));

            return builder;
        }
    }
}
