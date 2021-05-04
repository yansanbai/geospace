// Atom representing single character in specific text style.
using System;

namespace TexDrawLib
{
    public class CharAtom : CharSymbol
    {
        public static CharAtom Get(char character, int fontIndex)
        {
            var atom = ObjPool<CharAtom>.Get();
            atom.Character = character;
            atom.FontIndex = fontIndex == -1 ? TEXPreference.main.GetTypefaceFor(character) : fontIndex;

            // simple test
            var f = TEXPreference.main.fonts[atom.FontIndex];
            if (f.type != TexAssetType.Font && f.GetChar(character) == null)
                // a sprite. simply no way to fix this!
                throw new InvalidOperationException("Illegal Character! '" + character + "' doesn't exist in " + f.name);

            return atom;
        }

        public char Character;

        public int FontIndex;

        public override Box CreateBox()
        {
            return CharBox.Get(FontIndex, Character);
        }

        public override TexChar GetChar()
        {
            return TEXPreference.main.GetChar(FontIndex, Character);
        }

        public override void Flush()
        {
            Character = default(char);
            FontIndex = -1;
            ObjPool<CharAtom>.Release(this);
        }
    }
}
