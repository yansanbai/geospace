using System;
using UnityEngine;

namespace TexDrawLib
{
    public class TexMetaRenderer : IFlushable
    {
        public bool enabled;
        // ------- These props can and have to be handled before boxing ---------

        /// -1 if use default, 1 = Bold, 2 = Italic, 4 = Underline, 8 = Strikethough
        public int style = -1;

        /// -2 if use default, -1 if it math, others are font index
        public int font = -2;

        public float kerning;
        public float size;

        // ------- These props is handled only in DrawingParams ---------

        /// -1 if use default
        public int align = -1;

        /// 0 if use default, 1 if normal, 2 if reversed
        public int wrap;

        /// First line indentation of paragraph
        public float leading;

        /// Left margin (horizontal)
        public float left;

        /// Right margin (horizontal)
        public float right;

        /// Fixed Line Height
        public float line;

        /// Standard spacing
        public float spacing;

        /// Paragraph spacing
        public float paragraph;

        private static string[] longTokens = { "style", "font", "lead", "left", "right", "kern", "line", "space", "para", "size", "align", "wrap" };
        // private static char[] shortTokens = { 't', 'f', 'l', 'b', 'r', 'k', 'h', 'n', 'p', 's', 'x', 'w' };
        private static string[] styleTokens = { "n", "b", "i", "bi" };
        private static char[] alignTokens = { 'l', 'c', 'r' };
        private static char[] wrapTokens = { 'c', 'l', 'r' };

        public void Reset()
        {
            enabled = false;
            style = -1;
            font = -2;
            size = 0;
            leading = 0;
            left = 0;
            right = 0;
            kerning = 0;
            line = 0;
            spacing = 0;
            paragraph = 0;
            align = -1;
            wrap = 0;
        }

        public void ApplyBeforeBoxing()
        {
            if (!enabled)
                return;
            if (style != -1)
                TexContext.Style.value = (FontStyle)(style); //Only Bold or Italic
            if (kerning != 0)
                TexContext.Kerning.value = kerning;
        }

        public void ApplyBeforeBoxing(DrawingParams param)
        {
            if (!enabled)
                return;
            ApplyBeforeBoxing();
            // Return to param if not set
            if (style == -1)
                TexContext.Style.value = param.fontStyle; //Only Bold or Italic
            if (kerning == 0)
                TexContext.Kerning.value = 0;
        }

        public void ApplyBeforeParsing()
        {
            if (TexContext.FontMetaPushed)
                TexContext.Font.Pop();
            if (TexContext.FontMetaPushed = font != -2)
                TexContext.Font.Push(font);
        }

        public float GetAlignment(float def)
        {
            switch (align)
            {
                case 0: return 0;
                case 1: return .5f;
                case 2: return 1;
                default: return def;
            }
        }

        public bool GetWrappingReversed(bool def)
        {
            switch (wrap)
            {
                case 0: return def;
                case 1: return false;
                case 2: return true;
                default: return def;
            }
        }

        public void ParseString(string raw)
        {
            enabled = true;
            if (string.IsNullOrEmpty(raw))
                return;
            int s = 0, l = 0;
            while (l < raw.Length)
            {
                while (l < raw.Length && raw[l] != ' ')
                {
                    l++;
                }
                ParsePerToken(raw.Substring(s, l - s));
                s = ++l;
            }
        }

        private void ParsePerToken(string raw)
        {
            // evil sanitization
            var delimiter = raw.IndexOf('=');
            if (delimiter <= 0)
                throw new TexParseException("Expected a '=' or key in one of the meta token");
            else if (delimiter == raw.Length - 1)
                throw new TexParseException("Expected a value of token after '='");

            var k = raw.Substring(0, delimiter);
            var key = Array.IndexOf(longTokens, k);
            if (key < 0)
                throw new TexParseException("Token '" + k + "' is unknown. Available options: " + string.Join(", ", longTokens));

            var value = raw.Substring(delimiter + 1);

            switch (key)
            {
                // "style", "font", "lead", "left", "right", "kern", "space", "para", "size", "align", "wrap"
                case 0: style = T(value, styleTokens); break;
                case 1: font = FONT(value); break;
                case 2: leading = F(value); break;
                case 3: left = F(value); break;
                case 4: right = F(value); break;
                case 5: kerning = F(value); break;
                case 6: line = F(value); break;
                case 7: spacing = F(value); break;
                case 8: paragraph = F(value); break;
                case 9: size = F(value); break;
                case 10: align = T(value, alignTokens); break;
                case 11: wrap = T(value, wrapTokens); break;
            }
        }

        static private float F(string s) { return float.Parse(s); }

        static private int T(string s, string[] t)
        {
            var i = Array.IndexOf(t, s);
            if (i < 0) throw new TexParseException("Token not found: " + s + ". Available options: " + t.ToString());
            else return i;
        }

        static private int T(string s, char[] t)
        {
            var i = Array.IndexOf(t, s[0]);
            if (i < 0) throw new TexParseException("Token not found: " + s + ". Available options: " + t.ToString());
            else return i;
        }

        static private int FONT(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return i;
            else if (s == "math")
                return -1;
            else
            {
                i = TEXPreference.main.GetFontIndexByID(s);
                if (i < 0)
                    throw new TexParseException("Font token value is not found: " + s);
                else
                    return i;
            }
        }

        public void Flush()
        {
            Reset();
            ObjPool<TexMetaRenderer>.Release(this);
        }

        private bool m_flushed = false;
        public bool IsFlushed { get { return m_flushed; } set { m_flushed = value; } }

    }
}
