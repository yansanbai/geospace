using UnityEngine;

namespace TexDrawLib
{
    // Atom representing radical (nth-root) construction.
    public class Radical : Atom
    {
        private const string rootSymbol = "surdsign";

        private const float scale = 0.65f;

        public static Radical Get(Atom baseAtom)
        {
            return Get(baseAtom, null);
        }

        public static Radical Get(Atom baseAtom, Atom degreeAtom)
        {
            var atom = ObjPool<Radical>.Get();
            atom.Type = CharTypeInternal.Inner;
            atom.BaseAtom = baseAtom;
            atom.DegreeAtom = degreeAtom;
            return atom;
        }

        public Atom BaseAtom;
        public Atom DegreeAtom;

        public override Box CreateBox()
        {
            // Calculate minimum clearance amount.
            if (BaseAtom == null)
                return StrutBox.Empty;

            // Create box for base atom, in cramped style.
            TexContext.Environment.Push(TexUtility.GetCrampedStyle());
            var baseBox = BaseAtom.CreateBox();
            TexContext.Environment.Pop();
            if (DegreeAtom is SymbolAtom && ((SymbolAtom)DegreeAtom).IsDelimiter)
                return CreateGenericRadicalBox(baseBox, ((SymbolAtom)DegreeAtom).Name);
            else
                return CreateBoxDefault(baseBox);
        }

        private Box CreateBoxDefault(Box baseBox)
        {
            var factor = TexContext.Scale;
            var clearance = TEXConfiguration.main.RootMargin * factor;
            var lineThickness = TEXConfiguration.main.LineThickness * factor;

            // Create box for radical sign.
            var totalHeight = baseBox.totalHeight;
            var radicalSignBox = DelimiterFactory.CreateBox(rootSymbol, totalHeight + clearance + lineThickness);

            // Add some clearance to left and right side
            baseBox = HorizontalBox.Get(baseBox, baseBox.width + clearance * 2, TexAlignment.Center);

            // Add half of excess height to clearance.
            lineThickness = Mathf.Max(radicalSignBox.height, lineThickness);
            clearance = radicalSignBox.totalHeight - totalHeight - lineThickness * 2;

            // Create box for square-root containing base box.
            TexUtility.CentreBox(radicalSignBox);
            var overBar = OverBar.Get(baseBox, clearance, lineThickness);
            TexUtility.CentreBox(overBar);
            var radicalContainerBox = HorizontalBox.Get(radicalSignBox);
            radicalContainerBox.Add(overBar);

            // If atom is simple radical, just return square-root box.
            if (DegreeAtom == null)
                return radicalContainerBox;

            // Atom is complex radical (nth-root).

            // Create box for root atom.
            TexContext.Environment.Push(TexUtility.GetRootStyle());
            var rootBox = DegreeAtom.CreateBox();
            TexContext.Environment.Pop();

            var bottomShift = scale * (radicalContainerBox.height + radicalContainerBox.depth);
            rootBox.shift = radicalContainerBox.depth - rootBox.depth - bottomShift;

            // Create result box.
            var resultBox = HorizontalBox.Get();

            // Add box for negative kern.
            TexContext.Environment.Push(TexEnvironment.Display);
            var negativeKern = SpaceAtom.Get(-((radicalSignBox.width) / 2f), 0, 0).CreateBox();
            TexContext.Environment.Pop();

            var xPos = rootBox.width + negativeKern.width;
            if (xPos < 0)
                resultBox.Add(StrutBox.Get(-xPos, 0, 0, 0));

            resultBox.Add(rootBox);
            resultBox.Add(negativeKern);
            resultBox.Add(radicalContainerBox);

            return resultBox;
        }

        private Box CreateGenericRadicalBox(Box baseBox, string genericSymbol)
        {
            float clearance;
            var lineThickness = TEXConfiguration.main.LineThickness * TexContext.Scale;
            clearance = lineThickness;

            // Create box for radical sign.
            var totalHeight = baseBox.totalHeight;
            var radicalSignBox = DelimiterFactory.CreateBox(genericSymbol, totalHeight + clearance + lineThickness);

            // Add half of excess height to clearance.
            //lineThickness = Mathf.Max(radicalSignBox.height, lineThickness);
            clearance = radicalSignBox.totalHeight - totalHeight - lineThickness * 2;

            // Create box for square-root containing base box.
            TexUtility.CentreBox(radicalSignBox);
            var overBar = OverBar.Get(baseBox, clearance, lineThickness);

            var expansion = radicalSignBox.width - CustomizedGenericDelimOffset(genericSymbol, radicalSignBox.totalHeight) * radicalSignBox.width;
            overBar.children[0].width += expansion;
            overBar.children[0].shift -= expansion;

            TexUtility.CentreBox(overBar);
            var radicalContainerBox = HorizontalBox.Get(radicalSignBox);
            radicalContainerBox.Add(overBar);

            // There is no generic root then ...
            return radicalContainerBox;
        }

        private const float kGenericDelimCoeff = 0.045f;

        private static float CustomizedGenericDelimOffset(string symbol, float height)
        {
            var coeff = Mathf.Clamp01(Mathf.InverseLerp(1, 3, height));
            switch (symbol)
            {
                case "rbrack":
                    return Mathf.LerpUnclamped(0.2f, 0.07f, coeff);
                case "rsqbrack":
                    return Mathf.LerpUnclamped(0.09f, 0.07f, coeff);
                //return 1;
                case "rbrace":
                    return Mathf.LerpUnclamped(0.12f, 0.2f, coeff);
                //return 1.5f;
                case "lbrack":
                    return Mathf.LerpUnclamped(0.8f, 0.91f, coeff);
                //	return 6.5f;
                case "lsqbrack":
                    return Mathf.LerpUnclamped(0.45f, 0.5f, coeff);
                //return 3;
                case "lbrace":
                    return Mathf.LerpUnclamped(0.8f, 0.78f, coeff);
                //return 9;
                case "mid":
                    return 0.48f;
                case "uparrow":
                case "downarrow":
                case "updownarrow":
                    return 0.48f;
                default:
                    return 1;
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            if (DegreeAtom != null)
            {
                DegreeAtom.Flush();
                DegreeAtom = null;
            }
            ObjPool<Radical>.Release(this);
        }
    }
}
