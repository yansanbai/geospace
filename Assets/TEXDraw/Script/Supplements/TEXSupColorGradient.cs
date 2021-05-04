using System.Text;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Color Gradient")]
    [TEXSupHelpTip("Give a custom color over text in given gradient")]
    public class TEXSupColorGradient : TEXPerCharacterBase
    {
        public Gradient gradient = new Gradient();
        public MixMode mixMode;
        private static readonly string[] modes = { "color", "clr", "mclr" };
        private const string repFormat = @"\{0}[{1}]{{{2}}}";

        private static StringBuilder s = new StringBuilder();

        protected override string Subtitute(string match, float factor)
        {
            return string.Format(repFormat, modes[(int)mixMode], RGBToHTML(gradient.Evaluate(factor)), match);
        }

        private string RGBToHTML(Color32 clr)
        {
            s.Remove(0, s.Length);
            s.Append('#');
            s.Append(clr.r.ToString("x2"));
            s.Append(clr.g.ToString("x2"));
            s.Append(clr.b.ToString("x2"));
            if (clr.a < 255)
                s.Append(clr.a.ToString("x2"));
            return s.ToString();
        }

        public enum MixMode
        {
            color = 0,
            clr = 1,
            mclr = 2
        }
    }
}
