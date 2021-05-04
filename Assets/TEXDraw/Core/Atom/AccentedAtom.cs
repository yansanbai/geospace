using UnityEngine;

namespace TexDrawLib
{
    // Atom representing base atom with accent above it.
    public class AccentedAtom : Atom
    {
        public static AccentedAtom Get(Atom baseAtom, string accentName)
        {
            var atom = ObjPool<AccentedAtom>.Get();
            atom.BaseAtom = baseAtom ?? SpaceAtom.Get(0, 0, 0);
            atom.AccentAtom = SymbolAtom.GetAtom(accentName);
            return atom;
        }

        public static AccentedAtom Get(Atom baseAtom, TexFormula accent)
        {
            var atom = ObjPool<AccentedAtom>.Get();
            atom.BaseAtom = baseAtom ?? SpaceAtom.Get(0, 0, 0);
            atom.AccentAtom = accent.RootAtom as SymbolAtom;
            return atom;
        }

        public static AccentedAtom Get(Atom baseAtom, Atom accent)
        {
            var atom = ObjPool<AccentedAtom>.Get();
            atom.BaseAtom = baseAtom ?? SpaceAtom.Get(0, 0, 0);
            atom.AccentAtom = accent as SymbolAtom;
            return atom;
        }

        // Atom over which accent symbol is placed.
        public Atom BaseAtom;

        // Atom representing accent symbol to place over base atom.
        public SymbolAtom AccentAtom;

        public override Box CreateBox()
        {
            // Create box for base atom.
            TexContext.Environment.Push(TexUtility.GetCrampedStyle());
            var baseBox = BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox();
            TexContext.Environment.Pop();

            // Find character of best scale for accent symbol.
            var acct = TEXPreference.main.GetChar(AccentAtom.Name).GetMetric();
            while (acct.width < baseBox.width && acct.ch.nextLargerExist)
            {
                acct.Flush();
                acct = acct.ch.nextLarger.GetMetric();
            }

            var resultBox = VerticalBox.Get();

            // Create and add box for accent symbol.
            var accentWidth = (acct.bearing + acct.italic) * .5f;
            acct.italic = accentWidth + (acct.width * .5f);
            acct.bearing = accentWidth - (acct.width * .5f);
            resultBox.Add(acct);

            resultBox.Add(StrutBox.Get(0, TEXConfiguration.main.AccentMargin * TexContext.Scale, 0, 0));

            // Centre and add box for base atom. Centre base box and accent box with respect to each other.
            var boxWidthsDiff = (baseBox.width - acct.width) / 2f;
            acct.shift = Mathf.Max(boxWidthsDiff, 0);
            if (boxWidthsDiff < 0)
                baseBox = HorizontalBox.Get(baseBox, acct.width, TexAlignment.Center);

            resultBox.Add(baseBox);

            // Adjust height and depth of result box.
            var depth = baseBox.depth;
            var totalHeight = resultBox.height + resultBox.depth;
            resultBox.depth = depth;
            resultBox.height = totalHeight - depth;

            return resultBox;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            if (AccentAtom != null)
            {
                AccentAtom.Flush();
                AccentAtom = null;
            }
            ObjPool<AccentedAtom>.Release(this);
        }
    }
}
