using System;
using System.Collections.Immutable;
using System.Globalization;
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
            var content = GetContent(obj, context);
            if (!(content is null))
            {
                renderer.Write(content);
            }
        }

        protected override Task WriteAsync(StringRender renderer, HelperToken obj, Context context)
        {
            Write(renderer, obj, context);
            return Task.CompletedTask;
        }

        public string GetContent(HelperToken obj, Context context)
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
                        var arg = args[i].ShouldAttemptContextLoad
                            ? context.Lookup(args[i].Value)
                            : args[i].Value;

                        arg = TryConvertTypeIfRequired(arg, args[i].Value, argumentTypes[i + 1]);

                        if (arg is null)
                        {
                            return null;
                        }

                        arr[i + 1] = arg;
                    }

                    return helper.Delegate.Method.Invoke(helper.Delegate.Target, arr) as string;
                }
            }
            return null;
        }

        private static object TryConvertTypeIfRequired(object value, string arg, Type type)
        {
            if (value is null)
            {
                // When lookup is null we should use the argument as passed
                value = arg;
            }

            var lookupType = value.GetType();

            if (lookupType == type)
            {
                return value;
            }

            if (type.IsAssignableFrom(lookupType))
            {
                return value;
            }

            try
            {
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch
            {
            }

            return null;
        }
    }
}
