using System;
using System.Collections.Immutable;

namespace Stubble.Helpers
{
    internal static class CompareHelper
    {
        internal static bool CompareImmutableArraysWithEquatable<T>(in ImmutableArray<T> arr1, in ImmutableArray<T> arr2)
            where T : IEquatable<T>
        {
            if (arr1 == arr2)
            {
                return true;
            }

            if (arr1.Length == arr2.Length)
            {
                for (var i = 0; i < arr1.Length; i++)
                {
                    if (!arr1[i].Equals(arr2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        internal static bool CompareImmutableArrays<T>(in ImmutableArray<T> arr1, in ImmutableArray<T> arr2)
        {
            if (arr1 == arr2)
            {
                return true;
            }

            if (arr1.Length == arr2.Length)
            {
                for (var i = 0; i < arr1.Length; i++)
                {
                    if (!arr1[i].Equals(arr2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
