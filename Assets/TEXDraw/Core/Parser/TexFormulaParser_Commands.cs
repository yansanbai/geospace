using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TexDrawLib
{
    public partial class TexFormulaParser
    {
        // these are lists of possible commands

        private static readonly HashSet<string> NotFamily = new HashSet<string>(new string[] {
            "not", "nnot", "hnot", "dnot", "unot", "onot", "vnot", "vnnot",
        });

        private static readonly HashSet<string> HoldFamily = new HashSet<string>(new string[] {
            "hold", "vhold", "thold", "bhold", "lhold", "rhold",
        });

        private static readonly HashSet<string> FracFamily = new HashSet<string>(new string[] {
            "frac", "nfrac", "lfrac", "rfrac", "nlfrac", "nrfrac",
            // Uncomment here if you want full control on fraction alignments
            /*
				"llfrac", "rrfrac", "nllfrac", "nrrfrac", "lrfrac", "rlfrac", "nlrfrac", "nrlfrac",
                "clfrac", "lcfrac", "crfrac", "clfrac", "nclfrac", "nlcfrac", "ncrfrac", "nclfrac",
            */
        });

        private static readonly HashSet<string> TableFamily = new HashSet<string>(new string[] {
            "table", "vtable", "ltable", "rtable",
         	// Uncomment here if you want full control on table alignments
            /*
				"llltable", "llctable", "llrtable", "lcltable", "lcctable", "lcrtable", "lrltable", "lrctable", "lrrtable",
            	"clltable", "clctable", "clrtable", "ccltable", "ccctable", "ccrtable", "crltable", "crctable", "crrtable",
            	"rlltable", "rlctable", "rlrtable", "rcltable", "rcctable", "rcrtable", "rrltable", "rrctable", "rrrtable",

				"vllltable", "vllctable", "vllrtable", "vlcltable", "vlcctable", "vlcrtable", "vlrltable", "vlrctable", "vlrrtable",
            	"vclltable", "vclctable", "vclrtable", "vccltable", "vccctable", "vccrtable", "vcrltable", "vcrctable", "vcrrtable",
            	"vrlltable", "vrlctable", "vrlrtable", "vrcltable", "vrcctable", "vrcrtable", "vrrltable", "vrrctable", "vrrrtable",
            */
        });

        public static readonly string[] commands = new string[]
        {
			// List of commands at a glance
			"trs", "mtrs", "ltrs", "root", "sqrt", "link", "ulink", "under",
            "over","vmatrix", "matrix", "meta", "color", "border", "vborder",
            "mclr", "clr", "bg", "vbg", "size", "text", "math", "style", "vlink",
        }.Concat(NotFamily).Concat(HoldFamily).Concat(FracFamily).Concat(TableFamily).ToArray();

        // HashSet have huge performance benefit when using Contains() for repeated times
        public static readonly HashSet<string> commandsKey = new HashSet<string>(commands);

        private Atom ProcessCommand(TexFormula formula, string value, ref int position, string command)
        {
            SkipWhiteSpace(value, ref position);
            if (position == value.Length)
                return null;

            switch (command)
            {
                case "meta":
                    return Meta(formula, value, ref position);
                case "root":
                case "sqrt":
                    return Root(formula, value, ref position);
                case "vmatrix":
                case "matrix":
                    return Matrix(value, ref position, command == "vmatrix");
                case "style":
                case "math":
                case "text":
                    var idx = command == "text" ? TEXConfiguration.main.Typeface_Text : (command == "math" ? -1 : -2);
                    var style = ParseStyle(value, ref position);
                    formula.Add(ParseFontStyle(value, ref position, idx, style), true);
                    return null; // add by not making a separate block (hence wrappable).
                case "clr":
                case "mclr":
                case "color":
                    formula.Add(Color(value, ref position, command == "color" ? 1 : (command == "clr" ? 0 : 2)), true);
                    return null; // same reason with \style.
                case "bg":
                case "vbg":
                    return BgColor(value, ref position, command == "bg");
                case "size":
                    return Size(value, ref position);
                case "link":
                case "ulink":
                case "vlink":
                    return Link(value, ref position, command == "ulink", command != "vlink");
                case "trs":
                case "mtrs":
                case "ltrs":
                    return TRS(value, ref position, command == "trs" ? 0 : (command == "mtrs" ? 1 : 2));
                case "under":
                case "over":
                    return Not(value, ref position, command == "under" ? 4 : 5, false);
                case "border":
                case "vborder":
                    return Border(value, ref position, command == "border");
            }

            if (NotFamily.Contains(command))
                return Not(value, ref position, ParseNotMode(command[0], command[1]), true);

            if (FracFamily.Contains(command))
            {
                int FracAlignT = 0, FracAlignB = 0;
                bool FracAlignN = true;
                string prefix = command.Substring(0, command.Length - 4);
                if (prefix.Length > 0)
                {
                    if (prefix[0] == 'n')
                    {
                        FracAlignN = false;
                        prefix = prefix.Substring(1);
                    }
                    if (prefix.Length == 1)
                    {
                        FracAlignT = fracP(prefix[0]);
                        FracAlignB = FracAlignT;
                    }
                    else if (prefix.Length == 2)
                    {
                        FracAlignT = fracP(prefix[0]);
                        FracAlignB = fracP(prefix[1]);
                    }
                }
                return Frac(value, ref position, FracAlignN, (TexAlignment)FracAlignT, (TexAlignment)FracAlignB);
            }

            if (HoldFamily.Contains(command))
            {
                TexAlignment align = TexAlignment.Center;
                bool vertical = false;

                holdP(command.Substring(0, command.Length - 3), ref vertical, ref align);

                string size = ReadInsideBracket(value, ref position);
                if (position < value.Length && value[position] == leftGroupChar)
                    return HolderAtom.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                        .ExtractRoot(), size, vertical, align);
                else
                    return HolderAtom.Get(null, size, vertical, align);
            }
            if (TableFamily.Contains(command))
            {
                bool vertical = false;
                int align = 1 + 8 + 64;
                string prefix = command.Substring(0, command.Length - 5);
                if (prefix.Length > 0)
                {
                    if (prefix[0] == 'v')
                    {
                        vertical = true;
                        prefix = prefix.Substring(1);
                    }
                    if (prefix.Length == 1)
                    {
                        var pref = fracP(prefix[0]);
                        align = System.Math.Max(1, pref * 2) + System.Math.Max(8, pref * 16) + System.Math.Max(64, pref * 128);
                    }
                    else if (prefix.Length == 3)
                    {
                        var pref0 = fracP(prefix[0]);
                        var pref1 = fracP(prefix[1]);
                        var pref2 = fracP(prefix[2]);
                        align = System.Math.Max(1, pref0 * 2) + System.Math.Max(8, pref1 * 16) + System.Math.Max(64, pref2 * 128);
                    }
                }

                int lineStyleH = 0, lineStyleV = 0;
                if (value[position] == leftBracketChar)
                {
                    string lineOpt;
                    int lineP = 0;
                    lineOpt = ReadGroup(value, ref position, leftBracketChar, rightBracketChar);
                    for (int i = 0; i < lineOpt.Length; i++)
                    {
                        if (!int.TryParse(lineOpt[i].ToString(), out lineP))
                            continue;
                        if (i >= 6)
                            break;
                        switch (i)
                        {
                            case 0:
                                lineStyleH += lineP >= 2 ? 17 : lineP;
                                break;
                            case 1:
                                lineStyleH += lineP >= 2 ? 10 : (lineP == 1 ? 2 : 0);
                                break;
                            case 2:
                                lineStyleH += lineP >= 1 ? 4 : 0;
                                break;
                            case 3:
                                lineStyleV += lineP >= 2 ? 17 : lineP;
                                break;
                            case 4:
                                lineStyleV += lineP >= 2 ? 10 : (lineP == 1 ? 2 : 0);
                                break;
                            case 5:
                                lineStyleV += lineP >= 1 ? 4 : 0;
                                break;
                        }
                    }
                    SkipWhiteSpace(value, ref position);
                }
                else
                {
                    lineStyleH = 7;
                    lineStyleV = 7;
                }

                return Table(value, ref position, align, lineStyleH, lineStyleV, vertical);
            }
            throw new TexParseException("Invalid command.");
        }

        private Atom Border(string value, ref int position, bool margin)
        {
            string clr = ReadInsideBracket(value, ref position);
            return AttrBorderAtom.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                .ExtractRoot(), clr, margin);
        }

        private static int fracP(char c)
        {
            switch (c)
            {
                case 'l': return 1;
                case 'c': return 0;
                case 'r': return 2;
                default: return 0;
            }
        }

        private static void holdP(string prefix, ref bool isVertical, ref TexAlignment align)
        {
            if (prefix.Length > 0)
            {
                switch (prefix[0])
                {
                    case 'v':
                        isVertical = true;
                        break;
                    case 'l':
                        align = TexAlignment.Left;
                        break;
                    case 'r':
                        align = TexAlignment.Right;
                        break;
                    case 'b':
                        align = TexAlignment.Bottom;
                        goto case 'v';
                    case 't':
                        align = TexAlignment.Top;
                        goto case 'v';
                }
            }
        }

        static public bool isCommandRegistered(string str)
        {
            return commandsKey.Contains(str);
        }

        private Atom Meta(TexFormula formula, string value, ref int position)
        {
            var metaRule = formula.AttachedMetaRenderer;
            if (metaRule == null)
                metaRule = formula.AttachedMetaRenderer = ObjPool<TexMetaRenderer>.Get();
            else
                metaRule.Reset();

            metaRule.ParseString(ReadInsideBracket(value, ref position));

            // because \meta[font=xxx] must done at parsing
            metaRule.ApplyBeforeParsing();

            return null;
        }

        private Atom Root(TexFormula formula, string value, ref int position)
        {
            TexFormula degreeFormula = Parse(ReadInsideBracket(value, ref position));
            return Radical.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                .ExtractRoot(), degreeFormula == null ? null : degreeFormula.ExtractRoot());
        }

        private Atom Matrix(string value, ref int position, bool vertical)
        {
            MatrixAtom matrixAtom = MatrixAtom.Get();
            List<List<Atom>> childs = matrixAtom.Elements;

            Atom parsedChild = (Parse(ReadGroup(
                value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot());
            childs.Add(ListPool<Atom>.Get());
            if (parsedChild == null)
                MatrixAtom.Last(childs).Add(SpaceAtom.Get());
            if (parsedChild is RowAtom)
            {
                List<Atom> el = ((RowAtom)parsedChild).Elements;
                if (vertical)
                    MatrixAtom.ParseMatrixVertical(el, childs);
                else
                    MatrixAtom.ParseMatrix(el, childs);
                el.Clear();
                ObjPool<RowAtom>.Release((RowAtom)parsedChild);
            }
            else
                MatrixAtom.Last(childs).Add(parsedChild);
            matrixAtom.Elements = childs;
            return matrixAtom;
        }

        private string ReadInsideBracket(string value, ref int position)
        {
            string r = null;
            if (value[position] == leftBracketChar)
            {
                r = ReadGroup(value, ref position, leftBracketChar, rightBracketChar);
                SkipWhiteSpace(value, ref position);
            }
            return r;
        }

        private FontStyle ParseStyle(string value, ref int position)
        {
            if (value[position] == leftBracketChar)
            {
                var str = ReadGroup(value, ref position, leftBracketChar, rightBracketChar);
                SkipWhiteSpace(value, ref position);
                return ParseStylePrefix(str);
            }
            return TexUtility.FontStyleDefault;
        }

        private Atom ParseFontStyle(string value, ref int position, int font = -2, FontStyle style = TexUtility.FontStyleDefault)
        {
            if (font != -2)
                TexContext.Font.Push(font);
            var parsed = Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot();
            if (font != -2)
                TexContext.Font.Pop();

            if (style != TexUtility.FontStyleDefault)
                parsed = AttrStyleAtom.Get(parsed, style);

            return parsed;
        }

        private Atom Color(string value, ref int position, int mixmode)
        {
            string clr = ReadInsideBracket(value, ref position);
            AttrColorAtom endColor;
            var startColor = AttrColorAtom.Get(clr, mixmode, out endColor);
            return InsertAttribute(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot(), startColor, endColor);
        }

        private Atom BgColor(string value, ref int position, bool margin)
        {
            string clr = ReadInsideBracket(value, ref position);
            return AttrBgAtom.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                .ExtractRoot(), clr, margin);
        }

        private Atom Size(string value, ref int position)
        {
            string sz = ReadInsideBracket(value, ref position);
            return AttrSizeAtom.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                .ExtractRoot(), sz);
        }

        private Atom Link(string value, ref int position, bool underline, bool margin)
        {
            string meta = ReadInsideBracket(value, ref position);
            string group = ReadGroup(value, ref position, leftGroupChar, rightGroupChar);
            return AttrLinkAtom.Get(Parse(group).ExtractRoot(),
                meta ?? group.Trim(), underline, margin);
        }

        private int ParseNotMode(char prefix, char second = '\0')
        {
            switch (prefix)
            {
                case 'n':
                    return 1;
                case 'h':
                    return 2;
                case 'd':
                    return 3;
                case 'u':
                    return 4;
                case 'o':
                    return 5;
                case 'v':
                    return (second == 'n') ? 7 : 6;
                default:
                    return 0;
            }
        }

        private Atom Not(string value, ref int position, int mode, bool margin)
        {
            string sz = ReadInsideBracket(value, ref position);
            return NegateAtom.Get(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar))
                .ExtractRoot(), mode, sz, margin);
        }

        private Atom TRS(string value, ref int position, int mode)
        {
            string trs = ReadInsideBracket(value, ref position);
            AttrTransformationAtom endTRS, startTRS = AttrTransformationAtom.Get(trs, mode, out endTRS);
            return InsertAttribute(Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot(),
                startTRS, endTRS);
        }

        private Atom Frac(string value, ref int position, bool delim, TexAlignment nom, TexAlignment denom)
        {
            if (position == value.Length)
                return null;
            Atom numeratorFormula = null, denominatorFormula = null;
            numeratorFormula = Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot();
            SkipWhiteSpace(value, ref position);
            if (position != value.Length)
                denominatorFormula = Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot();

            return FractionAtom.Get(numeratorFormula, denominatorFormula, delim, nom, denom);
        }

        private Atom Table(string value, ref int position, int align, int h, int v, bool vertical)
        {
            List<List<Atom>> childs = new List<List<Atom>>();
            MatrixAtom matrixAtom = ObjPool<MatrixAtom>.Get();
            matrixAtom.horizontalAlign = align;
            matrixAtom.horizontalLine = h;
            matrixAtom.verticalLine = v;

            Atom parsedChild = (Parse(ReadGroup(
                value, ref position, leftGroupChar, rightGroupChar)).ExtractRoot());
            childs.Add(ListPool<Atom>.Get());
            if (parsedChild == null)
                MatrixAtom.Last(childs).Add(SpaceAtom.Get());
            if (parsedChild is RowAtom)
            {
                List<Atom> el = ((RowAtom)parsedChild).Elements;
                if (!vertical)
                    MatrixAtom.ParseMatrix(el, childs);
                else
                    MatrixAtom.ParseMatrixVertical(el, childs);
                el.Clear();
                ObjPool<RowAtom>.Release((RowAtom)parsedChild);
            }
            else
                MatrixAtom.Last(childs).Add(parsedChild);
            matrixAtom.Elements = childs;
            return matrixAtom;
        }
    }
}
