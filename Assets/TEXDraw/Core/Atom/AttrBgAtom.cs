using UnityEngine;

namespace TexDrawLib
{
    public class AttrBgAtom : Atom
    {
        public static AttrBgAtom Get(Atom baseAtom, string color, bool margin)
        {
            var atom = ObjPool<AttrBgAtom>.Get();
            atom.baseAtom = baseAtom;
            atom.margin = margin;
            atom.Type = baseAtom.Type;
            atom.color = color == null ? Color.clear : AttrColorAtom.ParseColor(color);
            return atom;
        }

        public Color color = Color.white;
        public Atom baseAtom;
        public bool margin;

        public override Box CreateBox()
        {
            var result = HorizontalBox.Get();
            var margin = TEXConfiguration.main.BackdropMargin * TexContext.Scale * 2;
            var box = baseAtom.CreateBox();

            if (this.margin)
                box = VerticalBox.Get(HorizontalBox.Get(box, box.width + margin,
                    TexAlignment.Center), box.totalHeight + margin, TexAlignment.Center);

            var bg = HorizontalRule.Get(box.height, box.width, 0, box.depth, true);

            if (color != Color.clear)
                result.Add(AttrColorBox.Get(0, color));
            result.Add(bg);
            if (color != Color.clear)
                result.Add(AttrColorBox.Get(3, color));

            result.Add(StrutBox.Get(-box.width, 0, 0, 0));
            result.Add(box);

            return result;
        }

        public override void Flush()
        {
            base.Flush();
            color = Color.clear;
            if (baseAtom != null)
            {
                baseAtom.Flush();
                baseAtom = null;
            }
            ObjPool<AttrBgAtom>.Release(this);
        }
    }
}
