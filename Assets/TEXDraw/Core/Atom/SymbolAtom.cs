namespace TexDrawLib
{
    /// Atom representing symbol (non-alphanumeric character).
    public class SymbolAtom : CharSymbol
    {
        public static SymbolAtom GetAtom(string name, char tag = '\0')
        {
            return Get(TEXPreference.main.GetChar(name), tag);
        }

        public static SymbolAtom Get(TexChar ch, char tag)
        {
            if (ch == null)
                return null;
            var atom = ObjPool<SymbolAtom>.Get();
            atom.Type = ch.type;
            atom.Name = ch.symbolName;
            atom.IsDelimiter = (ch.extensionExist || ch.nextLargerExist) && !(ch.extensionHorizontal) &&
                (ch.type >= CharType.Relation && ch.type <= CharType.CloseDelimiter);
            atom.Character = ch;
            atom.TaggedChar = tag;
            return atom;
        }

        /*public static SymbolAtom Get (string name, CharType type, bool isDelimeter, string pair)
		{
            var atom = ObjPool<SymbolAtom>.Get();
            atom.Type = type;
            atom.Name = name;
            atom.IsDelimeter = isDelimeter;
            atom.PairName = pair;
            return atom;
		}*/

        public string Name;

        public string PairName;

        public TexChar Character;

        public char TaggedChar;

        public override Box CreateBox()
        {
            return CharBox.Get(Character);
        }

        public Box CreateBox(float minHeight)
        {
            return DelimiterFactory.CreateBox(Name, minHeight);
        }

        public override TexChar GetChar()
        {
            return Character;
        }

        public override void Flush()
        {
            ObjPool<SymbolAtom>.Release(this);
        }
    }
}
