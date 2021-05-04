namespace TexDrawLib
{
    public class AttrShiftAtom : Atom
    {
        public static AttrShiftAtom Get(Atom Base, string Offset)
        {
            float s;
            var atom = ObjPool<AttrShiftAtom>.Get();
            atom.baseAtom = Base;
            atom.offset = float.TryParse(Offset, out s) ? s : 0;
            atom.Type = Base.Type;
            return atom;
        }

        public Atom baseAtom;
        public float offset;

        public override Box CreateBox()
        {
            var box = baseAtom.CreateBox();
            box.shift -= offset;
            return HorizontalBox.Get(box);
        }

        public override void Flush()
        {
            base.Flush();
            if (baseAtom != null)
            {
                baseAtom.Flush();
                baseAtom = null;
            }
            ObjPool<AttrShiftAtom>.Release(this);
        }
    }
}
