using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TexDrawLib
{
    [TEXSupHelpTip("Translate specific syntax to what TEXDraw understand", true)]
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Translator", 16), ExecuteInEditMode]
    public class TEXSupTranslator : TEXDrawSupplementBase
    {
        // TODO: Make this tags costumizable in runtime
        public class HTMLTags
        {
            public string b = "\\style[b]";
            public string i = "\\style[i]";
            public string bi = "\\style[bi]";
            public string u = "\\under";
            public string s = "\\hnot";
            public string sub = "_";
            public string sup = "^";
            [Space]
            public string strong = "\\style[b]";
            public string em = "\\style[i]";
            public string del = "\\hnot";
            public string ins = "\\under";
            public string pre = "\\cmtt";
            public string code = "\\cmtt";
            public string kbd = "\\border";
            public string div = "\\hold";
            public string q = "\\border[1000] ";
            public string math = " \\math";
            [Space]
            public string li = "\\rhold[{0}]{{ \\bullet }}";
            public string h1 = "\\style[b]\\size[2.5]";
            public string h2 = "\\style[b]\\size[1.75]";
            public string h3 = "\\style[b]\\size[1.5]";
            public string h4 = "\\style[b]\\size[1.25]";
            public string h5 = "\\style[b]\\size[1.05]";
            public string h6 = "\\style[b]\\size[0.9]";
            [Space]
            public string border = "\\border{0}";
            public string color = "\\color[{0}]";
            public string size = "\\size[{0}]";
            public string font = "\\{0}";
            public string bg = "\\bg[{0}]";
            public string a = "\\ulink[{0}]";

            protected Dictionary<string, string> _data = new Dictionary<string, string>();

            public HTMLTags()
            {
                // transfer fields to _data
                foreach (var t in GetType().GetFields())
                    if (t.FieldType == typeof(string))
                        _data[t.Name] = (string)t.GetValue(this);
                knownTags.UnionWith(_data.Keys.ToArray());
            }

            public string this[string tag, string param]
            {
                get
                {
                    return string.IsNullOrEmpty(param) ? _data.GetValue(tag) : string.Format(_data.GetValue(tag), param);
                }
            }

            public HashSet<string> knownTags = new HashSet<string>();
        }

        public enum TranslateType
        {
            Default = 0,
            Plain = 1,
            RichTags = 2,
            Markdown = 3,
            Latex = 4,
        }

        public TranslateType syntax = TranslateType.Latex;

        [Header("Latex Config")]
        public bool discardSpaces = false;

        [System.NonSerialized]
        private StringBuilder b = new StringBuilder();

        [System.NonSerialized]
        protected HTMLTags tags = new HTMLTags();

        public override string ReplaceString(string original)
        {
            b.Length = 0;
            switch (syntax)
            {
                case TranslateType.RichTags:
                    TranslateRichTags(original, b);
                    break;
                case TranslateType.Latex:
                    TranslateLatex(original, b);
                    break;
                case TranslateType.Plain:
                    TranslatePlain(original, b);
                    break;
                case TranslateType.Markdown:
                    TranslateMarkdown(original, b);
                    break;
                default:
                    b.Append(original);
                    break;
            }
            return b.ToString();
        }

        /* ------------------- PLAIN & REAL LATEX ------------------------- */

        private void TranslatePlain(string str, StringBuilder dst)
        {
            var i = 0;
            while (i < str.Length)
            {
                var c = str[i++];
                if (TexFormulaParser.IsParserReserved(c))
                    dst.Append("\\" + c);
                else
                    dst.Append(c);
            }
        }

        private void TranslateLatex(string str, StringBuilder dst)
        {
            var i = 0;
            char n = '\0';
            while (i < str.Length)
            {
                var c = str[i++];
                n = i < str.Length ? str[i] : '\0';
                if (c == ' ')
                {
                    if (!discardSpaces)
                        dst.Append(c);
                }
                else if (c == '\\')
                {
                    if (TexFormulaParser.IsParserReserved(n))
                    {
                        dst.Append(c);
                        continue;
                    }
                    else if (char.IsLetter(n))
                    {
                        var cmd = TexFormulaParser.LookForAWord(str, ref i);
                        TexFormulaParser.SkipWhiteSpace(str, ref i);

                        // Symbol renames
                        if (cmd == "to")
                        {
                            dst.Append("\\rightarrow");
                            continue;
                        }
                        if (i >= str.Length)
                        {
                            dst.Append("\\" + cmd);
                            continue;
                        }
                        // Lim (or other funcs) goes to over/under if even there single script
                        if (cmd == "lim")
                        {
                            c = str[i];
                            if (c == '^' || c == '_')
                                dst.Append("\\" + cmd + c);
                            else
                                dst.Append("\\" + cmd);
                            continue;
                        }
                        // color but the id in braces
                        if (cmd == "color" && (str[i] == '{'))
                        {
                            var arg = TexFormulaParser.ReadGroup(str, ref i, '{', '}');
                            if (arg.IndexOf(' ') < 0)
                            {
                                dst.Append("\\color[" + arg + "]");
                            }
                            else
                                dst.Append("\\color{" + arg + "}");
                            continue;
                        }
                        // \over... or \under....
                        int ou = cmd.IndexOf("over") == 0 ? 2 : (cmd.IndexOf("under") == 0 ? 1 : 0);
                        if (ou != 0)
                        {
                            var second = cmd.Substring(ou == 2 ? 4 : 5);
                            if (second == "line")
                            {
                                dst.Append(ou == 2 ? "\\over" : "\\under");
                                continue;
                            }
                            c = str[i];
                            if (c == '{')
                            {
                                var arg1 = "{" + TexFormulaParser.ReadGroup(str, ref i, '{', '}') + "}";
                                if (second == "brace")
                                {
                                    // \overbrace or \underbrace
                                    if (i + 1 < str.Length)
                                    {
                                        c = str[i++]; n = str[i];
                                        if ((c == '^' || c == '_') && n == '{')
                                        {
                                            // If there's script we need to think another strategy
                                            var arg2 = TexFormulaParser.ReadGroup(str, ref i, '{', '}');
                                            var arg3 = ou == 2 ? "\\lbrace" : "\\rbrace";
                                            if (c == '^')
                                                dst.Append("\\nfrac{\\size[.]{" + arg2 + "}__" + arg3 + "}{" + arg1 + "}");
                                            else
                                                dst.Append("\\nfrac{" + arg1 + "}{\\size[.]{" + arg2 + "}^^" + arg3 + "}");
                                            continue;
                                        }
                                        else
                                            i--;
                                    }
                                }
                                dst.Append(arg1);
                                dst.Append(ou == 2 ? "^^" : "__");
                                dst.Append("\\" + second);
                                continue;
                            }
                        }

                        //Default behav
                        dst.Append("\\" + cmd);
                    }
                }
                else
                    dst.Append(c);
            }
        }

        /* ------------------- HTML / RICH TAG ------------------------- */

        private void TranslateRichTags(string str, StringBuilder dst)
        {
            int i = 0;
            while (i < str.Length)
            {
                var ii = i;
                var c = str[i++];
                var n = i < str.Length ? str[i] : '\0';
                if (c == '<' && char.IsLetterOrDigit(n))
                {
                    var cmd = LookForAWordOrDigit(str, ref i);
                    if (tags.knownTags.Contains(cmd))
                    {
                        i = ii;
                        GetRichTag(str, ref i, dst);
                        continue;
                    }
                }
                dst.Append(c);
            }
        }

        private void GetRichTag(string str, ref int i, StringBuilder dst)
        {
            if (i >= str.Length || str[i] != '<')
                return;
            i++;
            var head = LookForAWordOrDigit(str, ref i);
            TexFormulaParser.SkipWhiteSpace(str, ref i);
            var param = TexFormulaParser.ReadGroup(str, ref i, '<', '>');

            dst.Append(TransparseRichTag(head, param));
            dst.Append('{');

            var end = str.IndexOf("</" + head + ">", i);
            i++;

            if (end < 0)
                end = str.Length;
            while (i < end)
            {
                var ii = i;
                var c = str[i++];
                var n = i < str.Length ? str[i] : '\0';
                if (c == '<' && char.IsLetterOrDigit(n))
                {
                    var cmd = LookForAWordOrDigit(str, ref i);
                    if (tags.knownTags.Contains(cmd))
                    {
                        i = ii;
                        GetRichTag(str, ref i, dst);
                        continue;
                    }
                }
                dst.Append(c);
            }
            if (i < str.Length)
            {
                while (str[i++] != '>') { }
            }
            dst.Append('}');
        }

        private string TransparseRichTag(string head, string param)
        {
            if (param.Length > 1)
                param = param.Substring(1); // Avoid the '='
            return tags[head, param];
        }

        private static string LookForAWordOrDigit(string value, ref int position)
        {
            var startPosition = position;
            while (position < value.Length)
            {
                var ch = value[position];
                var isEnd = position == value.Length - 1;
                if (!char.IsLetterOrDigit(ch) || isEnd)
                {
                    // Escape sequence has ended.
                    if (char.IsLetterOrDigit(ch))
                        position++;
                    break;
                }
                position++;
            }
            return value.Substring(startPosition, position - startPosition);
        }

        /* ----------------------- MARKDOWN --------------------------- */

        /// <summary>
        /// $1: New line
        /// $2: Link
        /// $3: Header (Atx-style)
        /// $4: Quote
        /// $5: Unordered List
        /// $6: Bold/Italic
        /// $7: Code
        /// $8: Strikethough
        /// $9: Math
        /// $10: Border
        /// </summary>
        static Regex MDTOKENS = new Regex(@"(\n)|(\[.+?\]\(.+?\))|(?:^(#+) )|(^> )|(^ *[-+*] )|([\*_]{1,3})|(`)|(~~)|(\$\$ ?)|([\[\]]{2})", RegexOptions.Multiline);
        static Regex MDLINK = new Regex(@"\[(.+?)\]\((.+?)\)");
        static string[] MDTAG = { "i", "b", "bi" };

        private void TranslateMarkdown(string str, StringBuilder dst)
        {
            bool opened = false;
            dst.Append(MDTOKENS.Replace(str, (m) =>
            {
                var g = m.Groups;
                if (g[1].Success)
                {
                    opened = false;
                    return "\n";
                }
                else if (g[2].Success)
                    return MDLINK.Replace(g[2].Value, tags["a", "$2"] + "{$1}");
                else if (g[3].Success)
                    return tags["h" + g[3].Length, ""];
                else if (g[4].Success)
                    return tags["q", ""] + "{ ";
                else if (g[5].Success)
                    return tags["li", (g[5].Length / 2).ToString()];
                else if (g[6].Success)
                    return (opened = !opened) ? tags[MDTAG[g[6].Length-1], ""] + "{" : "}";
                else if (g[7].Success)
                    return (opened = !opened) ? tags["code", ""] + "{" : "}";
                else if (g[8].Success)
                    return (opened = !opened) ? tags["s", ""] + "{" : "}";
                else if (g[9].Success)
                    return (opened = !opened) ? tags["math", ""] + "{" : "}";
                else if (g[10].Success)
                    return (opened = !opened) ? tags["kbd", ""] + "{" : "}";
                return m.Value;
            }));
        }
    }
}
