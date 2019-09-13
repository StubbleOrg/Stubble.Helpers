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
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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
                            return;
                        }

                        arr[i + 1] = arg;
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }

            return null;
        }
    }
}
