using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stubble.Helpers
{
    public readonly struct HelperRef : IEquatable<HelperRef>
    {
        public HelperRef(Delegate @delegate)
        {
            Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));

            var @params = @delegate.Method.GetParameters();
            var builder = ImmutableArray.CreateBuilder<Type>(@params.Length);
            foreach (var arg in @params)
            {
                builder.Add(arg.ParameterType);
            }
            ArgumentTypes = builder.ToImmutable();
        }

        public Delegate Delegate { get; }

        public ImmutableArray<Type> ArgumentTypes { get; }

        public override bool Equals(object obj)
        {
            return obj is HelperRef @ref && Equals(@ref);
        }

        public bool Equals(HelperRef other)
        {
            return EqualityComparer<Delegate>.Default.Equals(Delegate, other.Delegate) &&
                   CompareHelper.CompareImmutableArrays(ArgumentTypes, other.ArgumentTypes);
        }

        public override int GetHashCode()
        {
            var hashCode = -1973005441;
            hashCode = hashCode * -1521134295 + EqualityComparer<Delegate>.Default.GetHashCode(Delegate);
            foreach (var type in ArgumentTypes)
            {
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            }
            return hashCode;
        }

        public static bool operator ==(HelperRef left, HelperRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HelperRef left, HelperRef right)
        {
            return !(left == right);
        }
    }
}
