using System;
using System.Collections.Generic;

namespace Stubble.Helpers
{
    public readonly struct HelperArgument : IEquatable<HelperArgument>
    {
        public HelperArgument(string value, bool shouldLoadFromContext)
        {
            Value = value;
            ShouldAttemptContextLoad = shouldLoadFromContext;
        }

        public HelperArgument(string value)
            : this (value, true)
        {
        }

        public string Value { get; }

        public bool ShouldAttemptContextLoad { get; }

        public override bool Equals(object obj)
        {
            return obj is HelperArgument argument && Equals(argument);
        }

        public bool Equals(HelperArgument other)
        {
            return Value == other.Value &&
                   ShouldAttemptContextLoad == other.ShouldAttemptContextLoad;
        }

        public override int GetHashCode()
        {
            var hashCode = -156647477;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + ShouldAttemptContextLoad.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(HelperArgument left, HelperArgument right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HelperArgument left, HelperArgument right)
        {
            return !(left == right);
        }
    }
}
