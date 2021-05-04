namespace TexDrawLib
{
    // Atom representing other atom vertically centered with respect to axis.
    public class VerticalCenteredAtom : Atom
    {
        public static VerticalCenteredAtom Get(Atom atom)
        {
            var Atom = ObjPool<VerticalCenteredAtom>.Get();
            Atom.BaseAtom = atom;
            return Atom;
        }

        public Atom BaseAtom;

        public override Box CreateBox()
        {
            var box = BaseAtom.CreateBox();

            // Centre box relative to horizontal axis.
            var totalHeight = box.height + box.depth;
            var axis = TEXConfiguration.main.AxisHeight * TexContext.Scale;
            box.shift = -(totalHeight / 2) - axis;

            return HorizontalBox.Get(box);
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<VerticalCenteredAtom>.Release(this);
        }
    }
}
