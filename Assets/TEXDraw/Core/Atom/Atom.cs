// Atom (smallest unit) of TexFormula.
namespace TexDrawLib
{
    public abstract class Atom : IFlushable
    {
        public CharType Type = CharType.Ordinary;

        public abstract Box CreateBox();

        public virtual void Flush() { Type = CharType.Ordinary; }

        public bool IsFlushed { get; set; }

        public virtual CharType LeftType { get { return Type; } }

        public virtual CharType RightType { get { return Type; } }
    }
}
