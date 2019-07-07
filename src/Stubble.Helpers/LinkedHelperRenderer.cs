using System.Collections.Immutable;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;

namespace Stubble.Helpers
{
    public class LinkedHelperRenderer : StringObjectRenderer<LinkedHelperTokens>
    {
        private readonly HelperTagRenderer _helperTagRender;

        public LinkedHelperRenderer(HelperTagRenderer helperTagRender)
        {
            _helperTagRender = helperTagRender;
        }

        protected override void Write(StringRender renderer, LinkedHelperTokens obj, Context context)
        {
            string result = null;
            for (var i = 0; i < obj.Tokens.Count; i++)
            {
                var token = obj.Tokens[i];
                if (i > 0)
                {
                    var args = ImmutableArray.CreateBuilder<HelperArgument>();
                    args.Add(new HelperArgument(result, false));
                    if (token.Args != null)
                    {
                        args.AddRange(token.Args);
                    }
                    token = token.Clone();
                    token.Args = args.ToImmutable();
                }
                result = _helperTagRender.GetContent(token, context);
            }

            if (!(result is null))
            {
                renderer.Write(result);
            }
        }

        protected override Task WriteAsync(StringRender renderer, LinkedHelperTokens obj, Context context)
        {
            Write(renderer, obj, context);
            return Task.CompletedTask;
        }
    }
}
