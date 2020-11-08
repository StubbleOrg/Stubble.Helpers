using System;
using Stubble.Core.Contexts;
using Stubble.Core.Exceptions;
using Stubble.Core.Settings;

namespace Stubble.Helpers
{
    public class HelperContext
    {
        private readonly Context _context;

        public HelperContext(Context context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            _context = context;
            RendererSettings = new HelperRendererSettings(_context.RendererSettings);
        }

        /// <summary>
        /// Gets the render settings for the context
        /// </summary>
        public RenderSettings RenderSettings => _context.RenderSettings;

        /// <summary>
        /// Gets the renderer settings for the context
        /// </summary>
        public HelperRendererSettings RendererSettings { get; }

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
