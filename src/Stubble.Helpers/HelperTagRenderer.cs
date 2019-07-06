using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;

namespace Stubble.Helpers
{
    public class HelperTagRenderer : StringObjectRenderer<HelperToken>
    {
        private readonly ImmutableDictionary<string, HelperRef> _helperCache;

        public HelperTagRenderer(ImmutableDictionary<string, HelperRef> helperCache)
        {
            _helperCache = helperCache;
        }

        protected override void Write(StringRender renderer, HelperToken obj, Context context)
        {
            if (_helperCache.TryGetValue(obj.Name, out var helper))
            {
                var helperContext = new HelperContext(context);
                var args = obj.Args;

                var argumentTypes = helper.ArgumentTypes;
                if ((argumentTypes.Length - 1) == args.Length)
                {
                    var arr = new object[args.Length + 1];
                    arr[0] = helperContext;

                    for (var i = 0; i < args.Length; i++)
                    {
                        if (args[i].Length > 0 && ((args[i][0] == '"' && args[i][args[i].Length - 1] == '"') || args[i][0] == '\'' && args[i][args[i].Length - 1] == '\''))
                        {
                            arr[i + 1] = Convert.ChangeType(args[i].Substring(1, args[i].Length - 2), argumentTypes[i + 1]);
                        }
                        else
                        {
                            var lookup = context.Lookup(args[i]);
                            if (lookup is null)
                            {
                                return;
                            }

                            if (argumentTypes[i + 1] == lookup.GetType())
                            {
                                arr[i + 1] = lookup;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    var result = helper.Delegate.Method.Invoke(helper.Delegate.Target, arr);
                    if (result is string str)
                    {
                        renderer.Write(str);
                    }
                }
            }
        }

        protected override Task WriteAsync(StringRender renderer, HelperToken obj, Context context)
        {
            Write(renderer, obj, context);
            return Task.CompletedTask;
        }
    }
}
