using System.Text.RegularExpressions;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Tab Aligner")]
    [TEXSupHelpTip("Manipulate Tab character with fixed alignment", true)]
    public class TEXSupTabAligner : TEXDrawSupplementBase
    {
        public enum TabAlignment
        {
            Auto = 0,
            LTR = 1,
            RTL = 2
        }

        private static readonly string m_formatLTR = "\\lhold[{0}]{{{1}}}{2}";
        private static readonly string m_formatRTL = "{2}\\rhold[{0}]{{{1}}}";

        [Tooltip("Give a hint for alignment. Auto is slowest but it can detect meta params.\nA fixed hint (LTR/RTL) is much prefered if you don't use Meta anyway")]
        public TabAlignment m_Alignment;

        public float m_GridWidth;
        public float[] m_CustomTabWidths;

        public override string ReplaceString(string original)
        {
            //Not the best way to go (GC Hugger)
            var arr = original.Split(new char[] { '\n' });
            var auto = m_Alignment == TabAlignment.Auto;
            var rtl = auto ? (tex.alignment.x > .5f) : (m_Alignment == TabAlignment.RTL);
            for (int i = 0; i < arr.Length; i++)
            {
                if (auto)
                {
                    // Meta param detection
                    var meta = Regex.Match(arr[i], @"\\meta\[[\w\W]*\]");
                    if (meta.Success)
                    {
                        var str = meta.Value.Substring(6, meta.Length - 7);
                        m_meta.Reset();
                        m_meta.ParseString(str);
                        rtl = m_meta.GetAlignment(tex.alignment.x) > .5f;
                    }
                }

                arr[i] = rtl ? ReplaceLinesRTL(arr[i]) : ReplaceLinesLTR(arr[i]);
            }
            return string.Join("\n", arr);
        }

        private string ReplaceLinesLTR(string str)
        {
            int m = 0, idx = 0;
            while (true)
            {
                idx = str.IndexOf('\t', idx);
                if (idx >= 0)
                {
                    var width = m < m_CustomTabWidths.Length ? m_CustomTabWidths[m] : m_GridWidth * (m + 1);
                    str = string.Format(m_formatLTR, width, str.Substring(0, idx), str.Substring(idx + 1));
                    idx += 11; // The number is relative to LTR format chars addition
                    m++;
                }
                else
                    break;
            }
            return str;
        }

        private string ReplaceLinesRTL(string str)
        {
            int m = 0, idx = str.Length - 1;
            while (true)
            {
                idx = str.LastIndexOf('\t', idx);
                if (idx >= 0)
                {
                    var width = m < m_CustomTabWidths.Length ? m_CustomTabWidths[m] : m_GridWidth * (m + 1);
                    str = string.Format(m_formatRTL, width, str.Substring(idx + 1), str.Substring(0, idx));
                    m++;
                }
                else
                    break;
            }
            return str;
        }

        private TexMetaRenderer m_meta;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_meta = new TexMetaRenderer();
        }
    }
}
