// Atom representing single character that can be marked as text symbol.
namespace TexDrawLib
{
    public abstract class CharSymbol : Atom
    {
        public CharSymbol()
        {
            IsTextSymbol = false;
            IsDelimiter = false;
        }

        public bool IsTextSymbol { get; set; }

        public bool IsDelimiter { get; set; }

        public abstract TexChar GetChar();
    }
}
