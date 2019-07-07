using System.Collections.Generic;
using Stubble.Core.Tokens;

namespace Stubble.Helpers
{
    public class LinkedHelperTokens : InlineToken<LinkedHelperTokens>, INonSpace
    {
        public LinkedHelperTokens(IList<HelperToken> tokens)
        {
            Tokens = tokens;
        }

        public IList<HelperToken> Tokens { get; }

        public override bool Equals(LinkedHelperTokens other)
        {
            if (Tokens.Count != other.Tokens.Count)
            {
                return false;
            }

            for (int i = 0; i < Tokens.Count; i++)
            {
                if (!Tokens[i].Equals(other.Tokens[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LinkedHelperTokens;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            var combinedHash = 5381;
            foreach (var token in Tokens)
            {
                CombineHashCode(token.GetHashCode());
            }
            return combinedHash;

            void CombineHashCode(int n) => combinedHash = ((combinedHash << 5) + combinedHash) ^ n;
        }
    }
}
