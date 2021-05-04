using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup To Small Case")]
    [TEXSupHelpTip("Turn small case to LARGE CASE with given scale")]
    public class TEXSupToSmallCase : TEXPerCharacterBase
    {
        [Range(0.2f, 2f)]
        public float smallFactor = 0.9f;

        private const string repFormat = @"\size[{0}]{{{1}}}";

        protected override string Subtitute(string match, float factor)
        {
            //TODO: Do string concat is faster than format?

            if (match.Length != 1)
                return match;
            if (char.IsLower(match[0]))
            {
                return string.Format(repFormat, smallFactor.ToString("0.##"), match.ToUpper());
            }
            else
                return match;
        }
    }
}
