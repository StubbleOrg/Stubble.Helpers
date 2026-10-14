using System;
using System.Collections.Immutable;

namespace Stubble.Helpers
{
    public readonly struct HelperStage : IEquatable<HelperStage>
    {
        public HelperStage(string name, ImmutableArray<HelperArgument> args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }

        public ImmutableArray<HelperArgument> Args { get; }

        public bool Equals(HelperStage other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && CompareHelper.CompareImmutableArraysWithEquatable(Args, other.Args);
        }

        public override bool Equals(object obj)
        {
            return obj is HelperStage stage && Equals(stage);
        }

        public override int GetHashCode()
        {
            var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Name ?? string.Empty);
            if (!Args.IsDefault)
            {
                foreach (var argument in Args)
                {
                    hashCode = unchecked(hashCode * -1521134295 + argument.GetHashCode());
                }
            }

            return hashCode;
        }
    }
}
