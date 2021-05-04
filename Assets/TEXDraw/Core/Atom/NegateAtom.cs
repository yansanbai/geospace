//Atom for Creating Diagonal Negate Line
namespace TexDrawLib
{
    public class NegateAtom : Atom
    {
        public int mode = 0;

        public float offsetM = 0;
        public float offsetP = 0;
        public bool useMargin = true;

        public static NegateAtom Get(Atom baseAtom, int Mode, string Offset, bool UseMargin)
        {
            var atom = ObjPool<NegateAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.mode = Mode;
            atom.useMargin = UseMargin;
            atom.Type = CharTypeInternal.Inner;
            if (Offset != null)
            {
                int pos = Offset.IndexOf('-');
                if (pos < 0 || !float.TryParse(Offset.Substring(pos), out atom.offsetM))
                    atom.offsetM = 0;
                if (pos < 1 || !float.TryParse(Offset.Substring(0, pos), out atom.offsetP))
                {
                    if (pos == 0 || !float.TryParse(Offset, out atom.offsetP))
                        atom.offsetP = 0;
                }
            }
            else
            {
                atom.offsetM = 0;
                atom.offsetP = 0;
            }
            return atom;
        }

        public Atom BaseAtom;

        public override Box CreateBox()
        {
            if (BaseAtom == null)
                return StrutBox.Empty;
            var factor = TexContext.Scale / 2;
            var margin = useMargin ? TEXConfiguration.main.NegateMargin * factor : 0;
            var thick = TEXConfiguration.main.LineThickness * factor;
            var baseBox = BaseAtom.CreateBox();
            var result = HorizontalBox.Get();

            var negateBox = StrikeBox.Get(baseBox.height, baseBox.width, baseBox.depth,
                                margin, thick, (StrikeBox.StrikeMode)mode, offsetM, offsetP);
            negateBox.shift = baseBox.shift;
            result.Add(negateBox);
            result.Add(StrutBox.Get(-baseBox.width, 0, 0, 0));
            result.Add(baseBox);
            return result;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<NegateAtom>.Release(this);
        }
    }
}
