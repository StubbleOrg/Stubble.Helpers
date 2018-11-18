using Stubble.Core.Imported;
using Stubble.Core.Tokens;

namespace Stubble.Helpers
{
    public class HelperToken : InlineToken<HelperToken>, INonSpace
    {
        public string Name { get; set; }
        public string[] Args { get; set; }

        public override bool Equals(HelperToken other) =>
            (other.TagStartPosition, other.TagEndPosition, other.ContentStartPosition, other.ContentEndPosition, other.IsClosed)
                == (TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, IsClosed)
            && other.Content.Equals(Content)
            && other.Name.Equals(Name)
            && other.Args.Equals(Args);

        public override bool Equals(object obj)
            => obj is HelperToken a && Equals(a);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => (Name, Args, TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, Content, IsClosed).GetHashCode();
    }
}
