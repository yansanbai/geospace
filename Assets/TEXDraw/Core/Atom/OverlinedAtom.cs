// Atom representing other atom with horizontal rule above it.
namespace TexDrawLib
{
    public class OverlinedAtom : Atom
    {
        public static OverlinedAtom Get(Atom baseAtom)
        {
            var atom = ObjPool<OverlinedAtom>.Get();
            atom.Type = CharType.Ordinary;
            atom.BaseAtom = baseAtom;
            return atom;
        }

        public Atom BaseAtom;

        public override Box CreateBox()
        {
            // Create box for base atom, in cramped style.
            TexContext.Environment.Push(TexUtility.GetCrampedStyle());
            var baseBox = BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox();
            TexContext.Environment.Pop();

            // Create result box.
            var defaultLineThickness = TEXConfiguration.main.LineThickness * TexContext.Scale;
            var resultBox = OverBar.Get(baseBox, 3 * defaultLineThickness, defaultLineThickness);

            // Adjust height and depth of result box.
            resultBox.height = baseBox.height + 5 * defaultLineThickness;
            resultBox.depth = baseBox.depth;

            return resultBox;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<OverlinedAtom>.Release(this);
        }
    }
}
