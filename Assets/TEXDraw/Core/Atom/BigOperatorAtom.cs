using UnityEngine;

// Atom representing big operator with optional limits.
namespace TexDrawLib
{
    public class BigOperatorAtom : Atom
    {
        private static Box ChangeWidth(Box box, float maxWidth)
        {
            // Centre specified box in new box of specified width, if necessary.
            if (Mathf.Abs(maxWidth - box.width) > TexUtility.FloatPrecision)
                return HorizontalBox.Get(box, maxWidth, TexAlignment.Center);
            else
                return box;
        }

        public static BigOperatorAtom Get(Atom baseAtom, Atom lowerLimitAtom, Atom upperLimitAtom)
        {
            return Get(baseAtom, lowerLimitAtom, upperLimitAtom, false);
        }

        public static BigOperatorAtom Get(Atom baseAtom, Atom lowerLimitAtom, Atom upperLimitAtom, bool makeSuperScript)
        {
            var atom = ObjPool<BigOperatorAtom>.Get();
            atom.Type = baseAtom.Type;
            atom.BaseAtom = baseAtom;
            atom.LowerLimitAtom = lowerLimitAtom;
            atom.UpperLimitAtom = upperLimitAtom;
            atom.MakeSuperScripts = makeSuperScript;
            return atom;
        }

        // Atom representing big operator.
        public Atom BaseAtom;

        // Atoms representing lower and upper limits.
        public Atom LowerLimitAtom;

        public Atom UpperLimitAtom;

        // True then script will drawn smaller.
        public bool MakeSuperScripts;

        public override Box CreateBox()
        {
            // Create box for base atom.
            Box baseBox;
            float delta;

            if (BaseAtom is SymbolAtom && BaseAtom.Type == CharType.BigOperator)
            {
                // Find character of best scale for operator symbol.
                var opChar = TEXPreference.main.GetChar(((SymbolAtom)BaseAtom).Name).GetMetric();
                if (TexContext.Environment.value < TexEnvironment.Text && opChar.ch.nextLargerExist)
                {
                    opChar.Flush();
                    opChar = opChar.ch.nextLarger.GetMetric();
                }
                opChar.shift = -(opChar.height + opChar.depth) / 2;
                baseBox = HorizontalBox.Get(opChar);

                delta = opChar.bearing;
            }
            else
            {
                baseBox = HorizontalBox.Get(BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox());
                delta = 0;
            }

            // Create boxes for upper and lower limits.
            Box upperLimitBox, lowerLimitBox;
            TexContext.Environment.Push(MakeSuperScripts ? TexEnvironment.ScriptScript : TexUtility.GetSuperscriptStyle());
            if (UpperLimitAtom is SymbolAtom)
                upperLimitBox = DelimiterFactory.CreateBoxHorizontal(((SymbolAtom)UpperLimitAtom).Name, baseBox.width);
            else
                upperLimitBox = UpperLimitAtom == null ? null : UpperLimitAtom.CreateBox();
            TexContext.Environment.Pop();

            TexContext.Environment.Push(MakeSuperScripts ? TexEnvironment.ScriptScript : TexUtility.GetSubscriptStyle());
            if (LowerLimitAtom is SymbolAtom)
                lowerLimitBox = DelimiterFactory.CreateBoxHorizontal(((SymbolAtom)LowerLimitAtom).Name, baseBox.width);
            else
                lowerLimitBox = LowerLimitAtom == null ? null : LowerLimitAtom.CreateBox();
            TexContext.Environment.Pop();

            // Make all component boxes equally wide.
            var maxWidth = Mathf.Max(Mathf.Max(baseBox.width, upperLimitBox == null ? 0 : upperLimitBox.width),
                      lowerLimitBox == null ? 0 : lowerLimitBox.width);

            if (baseBox != null)
                baseBox = ChangeWidth(baseBox, maxWidth);
            if (upperLimitBox != null)
                upperLimitBox = ChangeWidth(upperLimitBox, maxWidth);
            if (lowerLimitBox != null)
                lowerLimitBox = ChangeWidth(lowerLimitBox, maxWidth);

            var resultBox = VerticalBox.Get();
            var opSpacing5 = TEXConfiguration.main.BigOpMargin * TexContext.Scale;
            var kern = 0f;

            // Create and add box for upper limit.
            if (UpperLimitAtom != null)
            {
                resultBox.Add(StrutBox.Get(0, opSpacing5, 0, 0));
                upperLimitBox.shift = delta / 2;
                upperLimitBox.shift += TopOffset(BaseAtom);
                resultBox.Add(upperLimitBox);
                kern = Mathf.Max(TEXConfiguration.main.BigOpUpShift * TexContext.Scale,
                    TEXConfiguration.main.BigOpUpperGap * TexContext.Scale - upperLimitBox.depth);
                resultBox.Add(StrutBox.Get(0, kern, 0, 0));
            }

            // Add box for base atom.
            resultBox.Add(baseBox);

            // Create and add box for lower limit.
            if (LowerLimitAtom != null)
            {
                resultBox.Add(StrutBox.Get(0, Mathf.Max(TEXConfiguration.main.BigOpLowShift * TexContext.Scale,
                            TEXConfiguration.main.BigOpLowerGap * TexContext.Scale - lowerLimitBox.height), 0, 0));
                lowerLimitBox.shift = -delta / 2;
                lowerLimitBox.shift += BottomOffset(BaseAtom);
                resultBox.Add(lowerLimitBox);
                resultBox.Add(StrutBox.Get(0, opSpacing5, 0, 0));
            }

            // Adjust height and depth of result box.
            var baseBoxHeight = baseBox.height;
            var totalHeight = resultBox.height + resultBox.depth;
            if (upperLimitBox != null)
                baseBoxHeight += opSpacing5 + kern + upperLimitBox.height + upperLimitBox.depth;
            resultBox.height = baseBoxHeight;
            resultBox.depth = totalHeight - baseBoxHeight;
            return resultBox;
        }

        public float TopOffset(Atom symbol)
        {
            if (!(symbol is SymbolAtom))
                return 0;
            var name = ((SymbolAtom)symbol).Name;
            switch (name)
            {
                case "int":
                case "oint":
                    return .6f;
                case "varint":
                case "varoint":
                case "iint":
                case "iiint":
                case "oiint":
                case "oiiint":
                    return .3f;
                default:
                    return 0;
            }
        }

        public float BottomOffset(Atom symbol)
        {
            if (!(symbol is SymbolAtom))
                return 0;
            var name = ((SymbolAtom)symbol).Name;
            switch (name)
            {
                case "int":
                case "oint":
                    return -.15f;
                case "varint":
                case "varoint":
                case "iint":
                case "iiint":
                case "oiint":
                case "oiiint":
                    return -.1f;
                default:
                    return 0;
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            if (LowerLimitAtom != null)
            {
                LowerLimitAtom.Flush();
                LowerLimitAtom = null;
            }
            if (UpperLimitAtom != null)
            {
                UpperLimitAtom.Flush();
                UpperLimitAtom = null;
            }
            ObjPool<BigOperatorAtom>.Release(this);
        }
    }
}
