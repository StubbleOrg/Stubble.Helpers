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
                        var lookup = context.Lookup(args[i]);
                        lookup = TryConvertTypeIfRequired(lookup, argumentTypes[i + 1]);

                        if (lookup is null)
                        {
                            return;
                        }

                        arr[i + 1] = lookup;
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

        public static object TryConvertTypeIfRequired(object lookup, Type type)
        {
            if (lookup is null)
            {
                return null;
            }

            var lookupType = lookup.GetType();

            if (lookupType == type)
            {
                return lookup;
            }

            if (type.IsAssignableFrom(lookupType))
            {
                return lookup;
            }

            try
            {
                return Convert.ChangeType(lookup, type);
            }
            catch
            {
            }

            return null;
        }
    }
}
