using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stubble.Helpers
{
    public class Helpers
    {
        private readonly Dictionary<string, HelperRef> _helpers
            = new Dictionary<string, HelperRef>();

        public ImmutableDictionary<string, HelperRef> HelperMap => _helpers.ToImmutableDictionary();

        public Helpers Register(string name, Func<HelperContext, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2>(string name, Func<HelperContext, T2, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3>(string name, Func<HelperContext, T2, T3, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4>(string name, Func<HelperContext, T2, T3, T4, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5>(string name, Func<HelperContext, T2, T3, T4, T5, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6>(string name, Func<HelperContext, T2, T3, T4, T5, T6, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, string> func) => Register(name, (Delegate)func);
        public Helpers Register<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string name, Func<HelperContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, string> func) => Register(name, (Delegate)func);

        private Helpers Register(string name, Delegate @delegate)
        {
            if (string.IsNullOrEmpty(name)) 
            {
                throw new ArgumentNullException(nameof(name));
            }

            _helpers[name.Trim()] = new HelperRef(@delegate);
            return this;
        }
    }
}
