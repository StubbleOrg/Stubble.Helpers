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

            object result;
            if (obj.InitialValue.HasValue)
            {
                var initialValue = obj.InitialValue.Value;
                result = initialValue.ShouldAttemptContextLoad
                    ? context.Lookup(initialValue.Value)
                    : initialValue.Value;
                if (result is null)
                {
                    return;
                }
            }
            else if (!TryInvokeHelper(obj.Name, obj.Args, null, false, context, out result))
            {
                return;
            }

            if (!obj.Pipeline.IsDefault)
            {
                foreach (var stage in obj.Pipeline)
                {
                    if (!TryInvokeHelper(stage.Name, stage.Args, result, true, context, out result))
                    {
                        return;
                    }
                }
            }

            var value = result is string str
                ? str
                : Convert.ToString(result, context.RenderSettings.CultureInfo);

            if (!context.RenderSettings.SkipHtmlEncoding && obj.EscapeResult && value is object)
            {
                value = context.RendererSettings.EncodingFuction(value);
            }

            renderer.Write(value);
        }

        protected override Task WriteAsync(StringRender renderer, HelperToken obj, Context context)
        {
            Write(renderer, obj, context);
            return Task.CompletedTask;
        }

        private static object TryConvertTypeIfRequired(object value, string arg, Type type)
        {
            if (value is null && !type.IsValueType)
            {
                return null;
            }
            else if (value is null)
            {
                // When lookup is null and type is not a string we should try convert since may be a constant integer or float.
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

        private bool TryInvokeHelper(string name, ImmutableArray<HelperArgument> args, object pipedValue, bool hasPipedValue, Context context, out object result)
        {
            result = null;
            if (!_helperCache.TryGetValue(name, out var helper))
            {
                return false;
            }

            var explicitArgumentCount = args.Length + (hasPipedValue ? 1 : 0);
            var argumentTypes = helper.ArgumentTypes;
            if (argumentTypes.Length - 1 != explicitArgumentCount)
            {
                return false;
            }

            var callArguments = new object[explicitArgumentCount + 1];
            callArguments[0] = new HelperContext(context);
            var argumentIndex = 1;
            if (hasPipedValue)
            {
                if (pipedValue is null)
                {
                    var pipedArgumentType = argumentTypes[argumentIndex];
                    if (pipedArgumentType.IsValueType && Nullable.GetUnderlyingType(pipedArgumentType) is null)
                    {
                        return false;
                    }

                    callArguments[argumentIndex++] = null;
                }
                else
                {
                    var convertedValue = TryConvertTypeIfRequired(pipedValue, null, argumentTypes[argumentIndex]);
                    if (convertedValue is null)
                    {
                        return false;
                    }

                    callArguments[argumentIndex++] = convertedValue;
                }
            }

            for (var index = 0; index < args.Length; index++)
            {
                var argument = args[index];
                var value = argument.ShouldAttemptContextLoad
                    ? context.Lookup(argument.Value)
                    : argument.Value;

                value = TryConvertTypeIfRequired(value, argument.Value, argumentTypes[argumentIndex]);
                if (value is null)
                {
                    return false;
                }

                callArguments[argumentIndex++] = value;
            }

            result = helper.Delegate.Method.Invoke(helper.Delegate.Target, callArguments);
            return true;
        }
    }
}
