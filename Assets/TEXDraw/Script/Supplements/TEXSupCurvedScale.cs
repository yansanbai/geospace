using UnityEngine;

namespace TexDrawLib

{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Curved Scale")]
    [TEXSupHelpTip("Give a custom scale factor in given curve")]
    public class TEXSupCurvedScale : TEXPerCharacterBase
    {
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        public float multiplier = 1;
        private const string repFormat = @"\size[{0}]{{{1}}}";

        protected override string Subtitute(string match, float factor)
        {
            //TODO: Do string concat is faster than format?
            if (float.IsNaN(factor))
                factor = 0;
            return @"\size[" + (curve.Evaluate(factor) * multiplier).ToString()
                + @"]{" + match + "}";
        }
    }
}
