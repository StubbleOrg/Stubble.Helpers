using System;
using Stubble.Core.Contexts;
using Stubble.Core.Settings;
using Stubble.Core.Exceptions;

namespace Stubble.Helpers
{
    public class HelperContext
    {
        private readonly Context _context;

        public HelperContext(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the render settings for the context
        /// </summary>
        public RenderSettings RenderSettings { get;}

        /// <summary>
        /// Gets the registry for the context
        /// </summary>
        public RendererSettings RendererSettings { get; }

        /// <summary>
        /// Looks up a value by name from the context
        /// </summary>
        /// <param name="name">The name of the value to lookup</param>
        /// <exception cref="StubbleDataMissException">If ThrowOnDataMiss set then thrown on value not found</exception>
        /// <exception cref="InvalidCastException">If the type <typeparamref name="T"/> does not match the found type</exception>
        /// <returns>The value if found or null if not</returns>
        public T Lookup<T>(string name)
            => (T)_context.Lookup(name);

        public bool IsTruthyValue(object value)
            => _context.IsTruthyValue(value);
    }
}
