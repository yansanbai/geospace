using UnityEngine;

namespace TexDrawLib
{
    public class AttrStyleAtom : Atom
    {
        public static AttrStyleAtom Get(Atom baseAtom, string styleStr)
        {
            var atom = ObjPool<AttrStyleAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.Style = ParseFontStyle(styleStr);
            atom.Type = baseAtom.Type;
            return atom;
        }

        public static AttrStyleAtom Get(Atom baseAtom, FontStyle style)
        {
            var atom = ObjPool<AttrStyleAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.Style = style;
            return atom;
        }

        public static FontStyle ParseFontStyle(string prefix)
        {
            switch (prefix)
            {
                case "n":
                    return FontStyle.Normal;
                case "b":
                    return FontStyle.Bold;
                case "i":
                    return FontStyle.Italic;
                case "bi":
                    return FontStyle.BoldAndItalic;
                default:
                    return TexUtility.FontStyleDefault;
            }
        }

        public Atom BaseAtom;

        public FontStyle Style;

        public override Box CreateBox()
        {
            // This StyleBox doesn't need start..end block, since the style metric are saved directly
            if (BaseAtom == null)
                return StrutBox.Empty;
            else
            {
                if (Style != TexUtility.FontStyleDefault)
                    TexContext.Style.Push(Style);
                var box = BaseAtom.CreateBox();
                if (Style != TexUtility.FontStyleDefault)
                    TexContext.Style.Pop();
                return box;
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<AttrStyleAtom>.Release(this);
        }
    }
}
