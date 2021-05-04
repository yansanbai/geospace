using UnityEngine;

namespace TexDrawLib
{
    // Atom representing other atom with atoms optionally over and under it.
    public class UnderOverAtom : Atom
    {
        private static Box ChangeWidth(Box box, float maxWidth)
        {
            if (box != null && Mathf.Abs(maxWidth - box.width) > TexUtility.FloatPrecision)
                return HorizontalBox.Get(box, maxWidth, TexAlignment.Center);
            else
                return box;
        }

        public static UnderOverAtom Get(Atom baseAtom, Atom underOver, float underOverSpace,
                               bool underOverScriptSize, bool over)
        {
            var atom = ObjPool<UnderOverAtom>.Get();
            atom.BaseAtom = baseAtom;

            if (over)
            {
                atom.UnderAtom = null;
                atom.UnderSpace = 0;
                atom.UnderScriptSmaller = false;
                atom.OverAtom = underOver;
                atom.OverSpace = underOverSpace;
                atom.OverScriptSmaller = underOverScriptSize;
            }
            else
            {
                atom.UnderAtom = underOver;
                atom.UnderSpace = underOverSpace;
                atom.UnderScriptSmaller = underOverScriptSize;
                atom.OverSpace = 0;
                atom.OverAtom = null;
                atom.OverScriptSmaller = false;
            }
            return atom;
        }

        public static UnderOverAtom Get(Atom baseAtom, Atom under, float underSpace, bool underScriptSize,
                               Atom over, float overSpace, bool overScriptSize)
        {
            var atom = ObjPool<UnderOverAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.UnderAtom = under;
            atom.UnderSpace = underSpace;
            atom.UnderScriptSmaller = underScriptSize;
            atom.OverAtom = over;
            atom.OverSpace = overSpace;
            atom.OverScriptSmaller = overScriptSize;
            return atom;
        }

        public Atom BaseAtom;

        public Atom UnderAtom;

        public Atom OverAtom;

        // Kern between base and under atom.
        public float UnderSpace;

        // Kern between base and over atom.
        public float OverSpace;

        public bool UnderScriptSmaller;

        public bool OverScriptSmaller;

        public override Box CreateBox()
        {
            // Create box for base atom.
            var baseBox = BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox();

            // Create boxes for over and under atoms.
            Box overBox = null, underBox = null;
            var maxWidth = baseBox.width;

            if (OverAtom != null)
            {
                if (OverScriptSmaller)
                    TexContext.Environment.Do(TexUtility.GetSuperscriptStyle(), () => overBox = OverAtom.CreateBox());
                else
                    overBox = OverAtom.CreateBox();
                maxWidth = Mathf.Max(maxWidth, overBox.width);
            }

            if (UnderAtom != null)
            {
                if (UnderScriptSmaller)
                    TexContext.Environment.Do(TexUtility.GetSubscriptStyle(), () => underBox = UnderAtom.CreateBox());
                else
                    underBox = UnderAtom.CreateBox();
                maxWidth = Mathf.Max(maxWidth, underBox.width);
            }

            // Create result box.
            var resultBox = VerticalBox.Get();

            // Create and add box for over atom.
            if (OverAtom != null)
            {
                resultBox.Add(ChangeWidth(overBox, maxWidth));
                resultBox.Add(TexUtility.GetBox(SpaceAtom.Get(0, OverSpace, 0)));
            }

            // Add box for base atom.
            resultBox.Add(ChangeWidth(baseBox, maxWidth));

            float totalHeight = resultBox.height + resultBox.depth - baseBox.depth;

            // Create and add box for under atom.
            if (UnderAtom != null)
            {
                resultBox.Add(TexUtility.GetBox(SpaceAtom.Get(0, UnderSpace, 0)));
                resultBox.Add(ChangeWidth(underBox, maxWidth));
            }

            resultBox.depth = resultBox.height + resultBox.depth - totalHeight;
            resultBox.height = totalHeight;

            return resultBox;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            if (OverAtom != null)
            {
                OverAtom.Flush();
                OverAtom = null;
            }
            if (UnderAtom != null)
            {
                UnderAtom.Flush();
                UnderAtom = null;
            }
            ObjPool<UnderOverAtom>.Release(this);
        }
    }
}
