using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;

namespace Stubble.Helpers
{
    public class HelperTagRenderer : StringObjectRenderer<HelperToken>
    {
        private ImmutableDictionary<string, Delegate> _helperCache;

        public HelperTagRenderer(ImmutableDictionary<string, Delegate> helperCache)
        {
            _helperCache = helperCache;
        }

        protected override void Write(StringRender renderer, HelperToken obj, Context context)
        {
            if (_helperCache.TryGetValue(obj.Name, out var helper))
            {
                var helperContext = new HelperContext(context);
                var args = obj.Args;

                var argumentCount = helper.GetType().GetGenericArguments();
                if ((argumentCount.Length - 2) == args.Length) 
                {
                    var arr = new object[args.Length + 1];
                    arr[0] = helperContext;
                    
                    for (var i = 0; i < args.Length; i++)
                    {
                        var lookup = context.Lookup(args[i]);
                        if (argumentCount[i + 1] == lookup.GetType())
                        {
                            arr[i + 1] = lookup;
                        }
                    }

                    var result = helper.Method.Invoke(helper.Target, arr);
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
