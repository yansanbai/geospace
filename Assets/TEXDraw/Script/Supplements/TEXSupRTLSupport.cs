using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup RTL Support")]
#if TEXDRAW_ARABICSUPPORT || TEXDRAW_PERSIANFIXER
	[TEXSupHelpTip ("Reverse the character to Support RTL language with given algorithm")]
#else
    [TEXSupHelpTip("Reverse the character to Support RTL language with given algorithm.\nEnable Arabic/Parsi integration (in Tools/TEXDraw) for more algorithm options")]
#endif
    public class TEXSupRTLSupport : TEXDrawSupplementBase
    {
        public enum RTLAlgorithm
        {
            CharacterReversal = 0,
#if TEXDRAW_ARABICSUPPORT
			ArabicSupportByAbdullahKonash = 1,
#endif
#if TEXDRAW_PERSIANFIXER
			PersianFixerByHamidMoghaddam = 2,
#endif
        }

        public enum DetectionOption
        {
            Depends = 0,
            AsLTR = 1,
            AsRTL = 2,
        }

#if TEXDRAW_ARABICSUPPORT
		public RTLAlgorithm m_Algorithm = RTLAlgorithm.ArabicSupportByAbdullahKonash;
#elif TEXDRAW_PERSIANFIXER
		public RTLAlgorithm m_Algorithm = RTLAlgorithm.PersianFixerByHamidMoghaddam;

#else
        public RTLAlgorithm m_Algorithm = RTLAlgorithm.CharacterReversal;
#endif

        [Tooltip("Bidirectional detection allows the supplement to filter which part of text is necessary to be converted by individual RTL algorithm.\nIt also contains code that specific to TEXDraw behaviour")]
        public bool m_Bidirectional = true;

        [Header("Bidirectional Detection Config")]
        [Tooltip("Supplement always determine the common order by first recognizable character. Override this only if something wrong or you want more consistent control")]
        public DetectionOption m_AssumeFirstChar = DetectionOption.Depends;

        [Tooltip("Assume any non-latin chars as ...")]
        public DetectionOption m_DetectNonLatin = DetectionOption.AsRTL;

        [Tooltip("Assume whitespace/non-printable chars as ...")]
        public DetectionOption m_DetectWhiteSpace = DetectionOption.Depends;

        [Tooltip("Assume number as ...")]
        public DetectionOption m_DetectNumber = DetectionOption.AsLTR;

        [Tooltip("Assume punctuation as ...")]
        public DetectionOption m_DetectPunctuation = DetectionOption.Depends;

        [NonSerialized]
        private StringBuilder m_builder = new StringBuilder();
        
        public override string ReplaceString(string original)
        {
            // Simple checks
            if (string.IsNullOrEmpty(original))
                return original;
            if (!m_Bidirectional)
                return Reverse(original);

            return Parse(original);
        }

        // This function can be called recursively
        private string Parse(string original)
        {
            if (string.IsNullOrEmpty(original))
                return original;

            original = original.Replace("\r", "");

            var b = new StringBuilder();

            int l = 0, e = 0, i = 0;
            bool onRTL = IsFirstRecognizableCharIsRTL(original, 0), commonlyRTL = onRTL;

            while (l < original.Length)
            {
                var c = original[l];
                var p = l == 0 ? '\0' : original[l - 1];
                var n = l == original.Length - 1 ? '\0' : original[l + 1];
                bool ignored = IsIgnoredChar(c);

                // If character is a new line
                if (c == '\n')
                {
                    if (e != l)
                    {
                        // This means something is left behind. Lets clear-em-up
                        Insert(b, original.Substring(e, l - e), onRTL, commonlyRTL, ref i);
                        e = l;
                    }
                    b.Append('\n');
                    e = ++l;
                    i = b.Length;
                    if (l < original.Length)
                        onRTL = commonlyRTL = IsFirstRecognizableCharIsRTL(original, l);
                }
                // If character is an opening brace
                else if (c == '{' && p != '\\' && n != '}' && n != '\0')
                {
                    if (e != l)
                    {
                        Insert(b, original.Substring(e, l - e), onRTL, commonlyRTL, ref i);
                        e = l;
                    }
                    var substring = TexFormulaParser.ReadGroup(original, ref l, '{', '}');
                    e = l;
                    var parsed = "{" + Parse(substring) + "}";
                    Insert(b, parsed, false, commonlyRTL, ref i);
                }
                // If character is a backslash (signals a command) (absolutely specific to TEXDraw behaviour)
                else if (c == '\\' && p != '\\' && char.IsLetter(n))
                {
                    if (e != l)
                    {
                        Insert(b, original.Substring(e, l - e), onRTL, commonlyRTL, ref i);
                        e = l;
                    }

                    l++;
                    var command = TexFormulaParser.LookForAWord(original, ref l);
                    var param1 = string.Empty;
                    var param2 = string.Empty;
                    if (l >= original.Length)
                    {
                        break;
                    }
                    else if (command.Contains("frac"))
                    {
                        param1 = "{" + Parse(TexFormulaParser.ReadGroup(original, ref l, '{', '}')) + "}";
                        param2 = "{" + Parse(TexFormulaParser.ReadGroup(original, ref l, '{', '}')) + "}";
                    }
                    else if (command == "rtl" || command == "ltr")
                    {
                        if (e != (l - 4))
                        {
                            Insert(b, original.Substring(e, (l - 4) - e), onRTL, commonlyRTL, ref i);
                        }
                        SkipWhiteSpace(original, ref l);
                        e = l;
                        onRTL = commonlyRTL = command == "rtl";
                        continue;
                    }
                    else if (TexFormulaParser.isCommandRegistered(command) || TEXPreference.main.GetFontIndexByID(command) >= 0)
                    {
                        if (original[l] == '[')
                        {
                            if (command == "root")
                                param1 = "[" + Parse(TexFormulaParser.ReadGroup(original, ref l, '[', ']')) + "]";
                            else
                                param1 = "[" + (TexFormulaParser.ReadGroup(original, ref l, '[', ']')) + "]";
                        }
                        if (l < original.Length && original[l] != '{' || !command.Contains("hold"))
                            param2 = "{" + Parse(TexFormulaParser.ReadGroup(original, ref l, '{', '}')) + "}";
                    }
                    var parsed = "\\" + command + param1 + param2;
                    Insert(b, parsed, false, commonlyRTL, ref i);
                    e = l;
                }
                // If character is a script sign (also specific to TEXDraw behaviour)
                else if (c == '_' || c == '^' && p != '\\')
                {
                    if (e != l)
                    {
                        Insert(b, original.Substring(e, l - e), onRTL, false, ref i);
                        e = l;
                    }

                    var iBackup = i;
                    while (c == '_' || c == '^')
                    {
                        l++;
                        var parsed = Parse(TexFormulaParser.ReadScriptGroup(original, ref l));
                        Insert(b, new string(c, 1) + parsed, onRTL, false, ref i);
                        c = l == original.Length ? '\0' : original[l];
                    }
                    e = l;
                    if (commonlyRTL)
                        i = iBackup;
                }
                // If character/space is different than current RTL mode
                else if ((IsRTLChar(c) != onRTL && !ignored) ||
                    (ignored && l < original.Length - 1 && (onRTL ^ commonlyRTL)
                    && ((IsRTLChar(c = original[l + 1])) != onRTL)))
                {
                    Insert(b, original.Substring(e, l - e), onRTL, commonlyRTL, ref i);

                    onRTL = IsRTLChar(c);
                    e = l++;
                }
                else
                    l++;
            }

            Insert(b, original.Substring(e), onRTL, commonlyRTL, ref i);

            var result = b.ToString();
            b.Length = 0;
            return result;
        }

        private void Insert(StringBuilder b, string substring, bool onRTL, bool commonlyRTL, ref int i)
        {
            var l = b.Length;
            b.Insert(i, onRTL ? Reverse(substring) : substring);

            if (!commonlyRTL)
                i += b.Length - l; // Must do this delta calc because sometimes like two char alif lam can be condensed to one char
        }

        static public void SkipWhiteSpace(string value, ref int position)
        {
            while (position < value.Length && (value[position] == ' '))
            {
                position++;
            }
        }

        private bool IsFirstRecognizableCharIsRTL(string s, int i)
        {
            if (m_AssumeFirstChar != DetectionOption.Depends)
                return m_AssumeFirstChar == DetectionOption.AsRTL;

            while (i < s.Length)
            {
                var c = s[i];
                i++;
                if (c == '\\')
                {
                    var f = TexFormulaParser.LookForAWord(s, ref i);
                    if (f.Length > 1 && i < s.Length && s[i] == '[')
                        TexFormulaParser.ReadGroup(s, ref i, '[', ']');
                    continue;
                }
                if (InternalChars.Contains(c) || IsIgnoredChar(c))
                    continue;
                return IsRTLChar(c);
            }
            return false;
        }

        private static readonly HashSet<char> InternalChars = new HashSet<char>(new char[] { '{', '}', '\\', '_', '^', '[', ']' });

        protected bool IsRTLChar(char c)
        {
            return (m_DetectNonLatin == DetectionOption.AsRTL && c > '\xFF') ||
            (m_DetectWhiteSpace == DetectionOption.AsRTL && char.IsWhiteSpace(c)) ||
            (m_DetectPunctuation == DetectionOption.AsRTL && char.IsPunctuation(c) && !InternalChars.Contains(c)) ||
            (m_DetectNumber == DetectionOption.AsRTL && char.IsDigit(c));
        }

        protected bool IsIgnoredChar(char c)
        {
            return (m_DetectNonLatin == DetectionOption.Depends && c > '\xFF') ||
            (m_DetectWhiteSpace == DetectionOption.Depends && char.IsWhiteSpace(c)) ||
            (m_DetectPunctuation == DetectionOption.Depends && char.IsPunctuation(c) && !InternalChars.Contains(c)) ||
            (m_DetectNumber == DetectionOption.Depends && char.IsDigit(c));
        }

        protected string Reverse(string original)
        {
            switch (m_Algorithm)
            {
                case RTLAlgorithm.CharacterReversal:
                    // Default useless-as-placeholder algoritm.
                    // Doing nothing other than reversing words.
                    m_builder.Length = 0;
                    int l = 0, e = 0;
                    while (l < original.Length)
                    {
                        e = l;
                        l = original.IndexOf('\n', e);
                        if (l == -1)
                            l = original.Length;
                        for (int i = l; i-- > e;)
                        {
                            m_builder.Append(original[i], 1);
                        }
                        l++;
                    }
                    return m_builder.ToString();
#if TEXDRAW_ARABICSUPPORT
			case RTLAlgorithm.ArabicSupportByAbdullahKonash:
                    // Arabic Support by Abdullah Konash
                    // https://github.com/Konash/arabic-support-unity/
				return ArabicSupport.ArabicFixer.Fix (original, true, true);
#endif
#if TEXDRAW_PERSIANFIXER
			case RTLAlgorithm.PersianFixerByHamidMoghaddam:
                    // Persian Fixer by Hamid Moghaddam
                    // https://github.com/HamidMoghaddam/unitypersiansupport/
				return UnityPersianSupport.PersianFixer.FixText (original);
#endif
                default:
                    return original;
            }
        }
    }
}
