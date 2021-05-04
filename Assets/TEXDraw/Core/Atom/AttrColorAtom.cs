using UnityEngine;

namespace TexDrawLib
{
    public class AttrColorAtom : Atom
    {
        public static AttrColorAtom Get(string colorStr, int mix, out AttrColorAtom endBlock)
        {
            var atom = ObjPool<AttrColorAtom>.Get();
            endBlock = ObjPool<AttrColorAtom>.Get();
            atom.EndAtom = endBlock;
            atom.mix = mix;
            endBlock.mix = mix;
            endBlock.color = atom.color = ParseColor(colorStr);
            return atom;
        }

        public AttrColorAtom EndAtom;

        public Color color = Color.white;
        public int mix;

        public override Box CreateBox()
        {
            return AttrColorBox.Get(EndAtom == null ? 3 : mix, color);
        }

        public override void Flush()
        {
            EndAtom = null;
            ObjPool<AttrColorAtom>.Release(this);
        }

        public static Color ParseColor(string str)
        {
            Color clr;
            if (str == null)
                clr = Color.white;
            else if (str.Length == 1)
                clr = ModifiedTerminalColor(str[0]);
            else if (!ColorUtility.TryParseHtmlString(str, out clr))
                if (!ColorUtility.TryParseHtmlString("#" + str, out clr))
                    clr = Color.white;

            return clr;
        }

        // Real CMD/Terminal color switch index
        public static Color terminalColor(char code)
        {
            switch (code)
            {
                case '0':
                    return new Color(0, 0, 0); // Black
                case '1':
                    return new Color(0, 0, .5f); // Blue
                case '2':
                    return new Color(0, .5f, 0); // Green
                case '3':
                    return new Color(0, .5f, .5f); // Aqua
                case '4':
                    return new Color(.5f, 0, 0); // Red
                case '5':
                    return new Color(.5f, 0, .5f); // Purple
                case '6':
                    return new Color(.5f, .5f, 0); // Yellow
                case '7':
                    return new Color(.8f, .8f, .8f); // White
                case '8':
                    return new Color(.5f, .5f, .5f); // Gray
                case '9':
                    return new Color(0, 0, 1f); // Light Blue
                case 'a':
                case 'A':
                    return new Color(0, 1f, 0); // Light Green
                case 'b':
                case 'B':
                    return new Color(0, 1f, 1f); // Light Aqua
                case 'c':
                case 'C':
                    return new Color(1f, 0, 0); // Light Red
                case 'd':
                case 'D':
                    return new Color(1f, 0, 1f); // Light Purple
                case 'e':
                case 'E':
                    return new Color(1f, 1f, 0); // Light Yellow
                case 'f':
                case 'F':
                    return new Color(1f, 1f, 1f); // Bright White
                default:
                    return new Color(1f, 1f, 1f);
            }
        }

        // Modifier version, from 0 (darkest), to f (lightest)
        // Sorted according to our eye spectrum : Blue, Red, then Green
        public static Color ModifiedTerminalColor(char code)
        {
            switch (code)
            {
                case '0':
                    return new Color(1f, 0, 0); // Light Red
                case '1':
                    return new Color(1, .5f, 0f); // Purple
                case '2':
                    return new Color(1f, 1f, 0); // Light Yellow
                case '3':
                    return new Color(.5f, 1f, 0); // Yellow
                case '4':
                    return new Color(0, 1f, 0); // Light Green
                case '5':
                    return new Color(0, 1f, .5f); // Aqua
                case '6':
                    return new Color(0, 1f, 1f); // Light Aqua
                case '7':
                    return new Color(0, .5f, 1f); // Red
                case '8':
                    return new Color(0, 0, 1f); // Light Blue
                case '9':
                    return new Color(.5f, 0, 1f); // Blue
                case 'a':
                case 'A':
                    return new Color(1f, 0, 1f); // Light Purple
                case 'b':
                case 'B':
                    return new Color(1, 0f, .5f); // Green
                case 'c':
                case 'C':
                    return new Color(0, 0, 0); // Black
                case 'd':
                case 'D':
                    return new Color(.2f, .2f, .2f); // Gray
                case 'e':
                case 'E':
                    return new Color(.7f, .7f, .7f); // White
                case 'f':
                case 'F':
                    return new Color(1f, 1f, 1f); // Bright White
                default:
                    return new Color(1f, 1f, 1f);
            }
        }
    }
}
