namespace TexDrawLib
{
    // Atom representing fraction, with or without separation line.
    public class FractionAtom : Atom
    {
        public static FractionAtom Get(Atom Numerator, Atom Denominator)
        {
            return Get(Numerator, Denominator, TexUtility.lineThickness, TexAlignment.Center, TexAlignment.Center);
        }

        public static FractionAtom Get(Atom Numerator, Atom Denominator, bool HasLine)
        {
            return Get(Numerator, Denominator, HasLine ? TexUtility.lineThickness : 0, TexAlignment.Center, TexAlignment.Center);
        }

        public static FractionAtom Get(Atom Numerator, Atom Denominator, float LineThickness)
        {
            return Get(Numerator, Denominator, LineThickness, TexAlignment.Center, TexAlignment.Center);
        }

        public static FractionAtom Get(Atom Numerator, Atom Denominator, bool HasLine,
            TexAlignment NumeratorAlignment, TexAlignment DenominatorAlignment)
        {
            return Get(Numerator, Denominator, HasLine ? TexUtility.lineThickness : 0, NumeratorAlignment, DenominatorAlignment);
        }

        public static FractionAtom Get(Atom Numerator, Atom Denominator, float LineThickness,
            TexAlignment NumeratorAlignment, TexAlignment DenominatorAlignment)
        {
            var atom = ObjPool<FractionAtom>.Get();
            atom.Type = CharTypeInternal.Inner;
            atom.numerator = Numerator;
            atom.denominator = Denominator;
            atom.numeratorAlignment = NumeratorAlignment;
            atom.denominatorAlignment = DenominatorAlignment;
            atom.lineThickness = LineThickness;
            return atom;
        }

        public Atom numerator;

        public Atom denominator;

        private TexAlignment numeratorAlignment;
        private TexAlignment denominatorAlignment;
        private float lineThickness;

        public override Box CreateBox()
        {
            float lineHeight = lineThickness * TexContext.Scale;

            // Create boxes for numerator and demoninator atoms, and make them of equal width.
            TexContext.Environment.Push(TexUtility.GetNumeratorStyle());
            var numeratorBox = numerator == null ? StrutBox.Empty : numerator.CreateBox();
            TexContext.Environment.Pop();

            TexContext.Environment.Push(TexUtility.GetDenominatorStyle());
            var denominatorBox = denominator == null ? StrutBox.Empty : denominator.CreateBox();
            TexContext.Environment.Pop();

            float maxWidth = (numeratorBox.width < denominatorBox.width ? denominatorBox.width : numeratorBox.width) + TEXConfiguration.main.FractionMargin * TexContext.Scale;
            numeratorBox = HorizontalBox.Get(numeratorBox, maxWidth, numeratorAlignment);
            denominatorBox = HorizontalBox.Get(denominatorBox, maxWidth, denominatorAlignment);

            // Calculate preliminary shift-up and shift-down amounts.
            float shiftUp, shiftDown;
            var styleFactor = TexContext.Scale;
            if (TexContext.Environment.value >= TexEnvironment.Text)
                styleFactor *= TEXConfiguration.main.FractionNarrowFactor;

            shiftUp = TEXConfiguration.main.NumeratorShift * styleFactor;
            shiftDown = TEXConfiguration.main.DenominatorShift * styleFactor;

            // Create result box.
            var resultBox = VerticalBox.Get();

            // add box for numerator.
            resultBox.Add(numeratorBox);

            // Calculate clearance and adjust shift amounts.
            //var axis = TEXConfiguration.main.AxisHeight * TexContext.Scale;

            // Calculate clearance amount.
            float clearance = lineHeight > 0 ? TEXConfiguration.main.FractionGap : TEXConfiguration.main.FractionGapNoLine;

            // Adjust shift amounts.
            var kern1 = shiftUp - numeratorBox.depth;
            var kern2 = shiftDown - denominatorBox.height;
            var delta1 = clearance - kern1;
            var delta2 = clearance - kern2;
            if (delta1 > 0)
            {
                shiftUp += delta1;
                kern1 += delta1;
            }
            if (delta2 > 0)
            {
                shiftDown += delta2;
                kern2 += delta2;
            }

            if (lineHeight > 0)
            {
                // Draw fraction line.

                resultBox.Add(StrutBox.Get(0, kern1, 0, 0));
                resultBox.Add(HorizontalRule.Get(lineHeight, numeratorBox.width, 0));
                resultBox.Add(StrutBox.Get(0, kern2, 0, 0));
            }
            else
            {
                // Do not draw fraction line.

                var kern = kern1 + kern2;
                resultBox.Add(StrutBox.Get(0, kern, 0, 0));
            }

            // add box for denominator.
            resultBox.Add(denominatorBox);

            // Adjust height and depth of result box.
            resultBox.height = shiftUp + numeratorBox.height;
            resultBox.depth = shiftDown + lineHeight + denominatorBox.depth;

            TexUtility.CentreBox(resultBox);
            return resultBox;
        }

        public override void Flush()
        {
            if (numerator != null)
            {
                numerator.Flush();
                numerator = null;
            }
            if (denominator != null)
            {
                denominator.Flush();
                denominator = null;
            }
            ObjPool<FractionAtom>.Release(this);
        }
    }
}
