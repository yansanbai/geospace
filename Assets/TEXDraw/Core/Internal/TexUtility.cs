#if (UNITY_STANDALONE || UNITY_ANDROID) && (ENABLE_MONO || ENABLE_DOTNET)
#define EMIT // Only Standalone or Android (with Mono backend) have access to Reflection Emit
#endif

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

#if EMIT
#endif

namespace TexDrawLib
{
    /// <summary>
    /// CSS-like intermediate context styling.
    /// Static for simplicity (assuming whole process is single-threaded).
    /// </summary>
    public static class TexContext
    {
        public class Param<T>
        {
            private T def;
            public T value;
            private Stack<T> stack = new Stack<T>();

            public void Push(T val) { stack.Push(value); value = val; }

            public void Pop() { value = stack.Pop(); }

            public Param(T def) { this.def = value = def; }

            public void Reset() { stack.Clear(); value = def; }

            public void Reset(T val) { stack.Clear(); value = val; }

            // performance jeez
            // public static implicit operator T(Param<T> t) { return t.value; }

            public void Do(T val, Action del)
            {
                Push(val); del(); Pop();
            }
        }

        /// <summary>
        /// Real size output
        /// </summary>
        public static float Scale
        {
            get
            {
                if (Environment.value < TexEnvironment.Script)
                    return Size.value;
                else if (Environment.value < TexEnvironment.ScriptScript)
                    return TEXConfiguration.main.ScriptFactor * Size.value;
                else
                    return TEXConfiguration.main.NestedScriptFactor * Size.value;
            }
        }

        public static int Resolution = 1; // no atom/box will change this
        public static Param<int> Font = new Param<int>(-1); // at parse
        public static Param<float> Size = new Param<float>(1); // at box
        public static Param<float> Kerning = new Param<float>(0); // at box
        public static Param<FontStyle> Style = new Param<FontStyle>(TexUtility.FontStyleDefault); // at box
        public static Param<TexEnvironment> Environment = new Param<TexEnvironment>(TexEnvironment.Display); // at box
        public static Param<Color32> Color = new Param<Color32>(new Color32(255, 255, 255, 255)); // at render

        public static bool FontMetaPushed = false; // simple placeholder

        public static void Reset()
        {
            FontMetaPushed = false;
            Size.Reset();
            Kerning.Reset();
            Font.Reset();
            Color.Reset();
            Style.Reset();
            Environment.Reset();
        }
    }

    public static class TexUtility
    {
        // Few const for simple adjusting ------------------------------------------------------------------------------

        public const float FloatPrecision = 0.001f;

        //The reason why it's 31 textures: because index 32 preserved for this block font!
        public const int blockFontIndex = 31;

        public static readonly Color white = Color.white; //Cached for speed
        public const FontStyle FontStyleDefault = (FontStyle)(-1);

        // Preserved Dynamic Configurations ----------------------------------------------------------------------------

        public static float spaceWidth { get { return TEXConfiguration.main.SpaceWidth; } }

        public static float spaceHeight { get { return TEXConfiguration.main.LineHeight; } }

        public static float spaceDepth { get { return TEXConfiguration.main.LineDepth; } }

        public static float spaceLine { get { return TEXConfiguration.main.LineSpace; } }

        public static float glueRatio { get { return TEXConfiguration.main.GlueRatio; } }

        public static float lineThickness { get { return TEXConfiguration.main.LineThickness; } }

        // TexStyle manipulations (for different style, etc.) ----------------------------------------------------------

        public static TexEnvironment GetCrampedStyle()
        {
            var Style = TexContext.Environment.value;
            return (int)Style % 2 == 1 ? Style : Style + 1;
        }

        public static TexEnvironment GetNumeratorStyle()
        {
            var Style = TexContext.Environment.value;
            return Style + 2 - 2 * ((int)Style / 6);
        }

        public static TexEnvironment GetDenominatorStyle()
        {
            var Style = TexContext.Environment.value;
            return (TexEnvironment)(2 * ((int)Style / 2) + 1 + 2 - 2 * ((int)Style / 6));
        }

        public static TexEnvironment GetRootStyle()
        {
            return TexEnvironment.Script;
        }

        public static TexEnvironment GetSubscriptStyle()
        {
            var Style = TexContext.Environment.value;
            return (TexEnvironment)(2 * ((int)Style / 4) + 4 + 1);
        }

        public static TexEnvironment GetSuperscriptStyle()
        {
            var Style = TexContext.Environment.value;
            return (TexEnvironment)(2 * ((int)Style / 4) + 4 + ((int)Style % 2));
        }

        public static void CentreBox(Box box)
        {
            float axis = TEXConfiguration.main.AxisHeight * TexContext.Scale;
            box.shift = (box.height - box.depth) / 2 - axis;
        }

        public static void AlignToBaseline(VerticalBox box, int baseIdx)
        {
            float offset = 0;
            int iter = -1;
            baseIdx = Mathf.Min(baseIdx, box.children.Count);
            while (iter++ < baseIdx)
            {
                if (iter == baseIdx)
                {
                    box.shift = offset;
                    return;
                }
                offset += box.children[iter].totalHeight;
            }
        }

        public static Box GetBox(Atom atom)
        {
            var box = atom.CreateBox();
            atom.Flush();
            return box;
        }

        public static Color MultiplyColor(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }

        public static Color32 MultiplyAlphaOnly(Color32 c, float a)
        {
            c.a = (byte)(c.a * a);
            return c;
        }

        public static FontStyle TexStyle2FontStyle(FontStyle style)
        {
            var s = (int)style;
            switch (Mathf.Min(s, s & 3))
            {
                case -1: case 0: return FontStyle.Normal;
                case 1: return FontStyle.Bold;
                case 2: return FontStyle.Italic;
                case 3: return FontStyle.BoldAndItalic;
                default: return FontStyle.Normal;
            }
        }

        public static string GetFontName(int idx)
        {
            if (idx >= 0)
                return TEXPreference.main.fonts[idx].name;
            return "text";
        }
    }

    [Serializable]
    public struct ScaleOffset
    {
        public float scale;
        public float offset;

        public ScaleOffset(float Scale, float Offset)
        {
            scale = Scale;
            offset = Offset;
        }

        public static ScaleOffset identity { get { return new ScaleOffset(1, 0); } }

        public float Evaluate(float v) { return v * scale + offset; }

        public Vector3 Evaluate(Vector3 v) { return v * scale + Vector3.one * offset; }
    }

    [Serializable]
    public class FindReplace
    {
        public string find;
        public string replace;

        [NonSerialized]
        private Regex cachedReg;

        [NonSerialized]
        private string cachedRegPattern;

        public string Execute(string text, bool regex)
        {
            if (string.IsNullOrEmpty(find))
                return text;

            if (regex)
            {
                if (cachedRegPattern != find)
                {
                    cachedReg = new Regex(find, RegexOptions.Multiline);
                    cachedRegPattern = find;
                }
                return cachedReg.Replace(text, replace);
            }
            else
                return text.Replace(find, replace);
        }
    }

    public static class ListExtensions
    {
        static public void EnsureCapacity<T>(this List<T> src, int cap)
        {
            if (src.Capacity < cap)
                src.Capacity = cap;
        }

        static public List<T> GetRangePool<T>(this List<T> source, int index, int count)
        {
            List<T> list = ListPool<T>.Get();

            list.Capacity = System.Math.Max(list.Capacity, count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i + index]);
            }

            return list;
        }
    }
}
