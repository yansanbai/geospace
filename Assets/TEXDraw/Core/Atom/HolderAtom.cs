using UnityEngine;

namespace TexDrawLib
{
    public class HolderAtom : Atom
    {
        public static HolderAtom Get(Atom baseAtom, Vector2 size, TexAlignment Alignment)
        {
            var atom = ObjPool<HolderAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.size = size;
            atom.align = Alignment;
            atom.Type = CharTypeInternal.Inner;
            return atom;
        }

        public static HolderAtom Get(Atom baseAtom, string size, bool vertical, TexAlignment Alignment)
        {
            Vector2 sz = new Vector2();
            if (size != null)
            {
                int idx = size.IndexOf(',');
                if (idx >= 0)
                {
                    var sizeL = size.Substring(0, idx);
                    var sizeR = size.Substring(idx + 1);
                    float.TryParse(sizeL, out sz.x);
                    float.TryParse(sizeR, out sz.y);
                }
                else
                {
                    if (vertical)
                        float.TryParse(size, out sz.y);
                    else
                        float.TryParse(size, out sz.x);
                }
            }
            return Get(baseAtom, sz, Alignment);
        }

        public Atom BaseAtom;

        public Vector2 size = Vector2.zero;
        public TexAlignment align;

        public override Box CreateBox()
        {
            var width = size.x;
            var height = size.y;

            Box result;
            if (BaseAtom == null)
                result = StrutBox.Get(width, height, 0, 0);
            else if (BaseAtom is SpaceAtom)
                result = StrutBox.Get(width, height, 0, 0);
            else if (width == 0 && BaseAtom is SymbolAtom)
                result = VerticalBox.Get(DelimiterFactory.CreateBox(((SymbolAtom)BaseAtom).Name, height), height, align);
            else if (height == 0 && BaseAtom is SymbolAtom)
                result = HorizontalBox.Get(DelimiterFactory.CreateBoxHorizontal(((SymbolAtom)BaseAtom).Name, width), width, align);
            else if (width == 0)
                result = VerticalBox.Get(BaseAtom.CreateBox(), height, align);
            else if (height == 0)
                result = HorizontalBox.Get(BaseAtom.CreateBox(), width, align);
            else
                result = VerticalBox.Get(HorizontalBox.Get(BaseAtom.CreateBox(), width, align), height, align);

            if (size.x + size.y > 1e-3f)
                TexUtility.CentreBox(result);
            return result;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<HolderAtom>.Release(this);
        }
    }
}
