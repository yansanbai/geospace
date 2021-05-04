using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TexDrawLib;
using System.Text.RegularExpressions;
using Block = TEXInput.Block;

[TEXSupHelpTip("Capture characters data for TEXInput")]
[AddComponentMenu("TEXDraw/TEXInput Logger")]
public class TEXInputLogger : TEXDrawSupplementBase
{

    [NonSerialized]
    internal List<Block> blocks = new List<Block>();

    public string emptyPlaceholder = "";

    private static Regex m_scriptEscape = new Regex(@"([\^_]{1,3})([\s!-/\[-`|-~:-@])", RegexOptions.ECMAScript);

    private static Regex m_wrapholder = new Regex(@"((?:\\vlink\[\d+\]{[^\s!-/\[-`{-~:-@]})+)", RegexOptions.ECMAScript);

    // m_pattern groups
    // #1: command name
    // #2: single character except meta characters
    // #3: escaped meta characters
    // #4: empty bracket (placeholder)
    // #5: symbol inside bracket {\symbol} for special lock
    private static Regex m_pattern = new Regex(@"(?:\\[a-zA-Z]+ *\[.*?[^\\]\])|(?:\\([a-zA-Z]+) *)|([^\\{}^_])|(\\[\\{}^_]?)|({})|(?:{{([^{].*?)}})", RegexOptions.ECMAScript);

    public override string ReplaceString(string original)
    {
        blocks.Clear();

        int line = 0;

        original = m_pattern.Replace(original, (match) =>
        {
            if (!match.Groups[5].Success && match.Length > 2)
            {
                // skip commands, fonts, big operators, delimiter
                var g = match.Groups[1];
                int s;
                var m = TEXPreference.main;
                if (!g.Success || TexFormulaParser.isCommandRegistered(g.Value)
                    || m.GetFontIndexByID(g.Value) >= 0)
                    return match.Value;
                else if ((s = m.symbols.GetValue(g.Value)) > 0 &&
                   (m.GetChar(s).nextLargerExist || m.GetChar(s).extensionExist))
                    return match.Value;
                else if (match.Length > g.Length + 1)
                {
                    // extra spaces can't be tolerated
                    // alternate to custom logic
                    return Group2WithSpaceElimination(match, g);
                }
            }

            var cmd = "\\vlink[" + blocks.Count + "]";
            Block dyn = new Block()
            {
                index = blocks.Count,
                start = match.Index,
                length = match.Length,
                // see text input cursor
                lineSeparator = -1,
            };

            if (match.Value == "\n")
            {
                dyn.lineSeparator = line++;
                blocks.Add(dyn);
                return "\n" + cmd + "{}";
            }
            else if (match.Value == "\\")
            {
                blocks.Add(dyn);
                return cmd + "{\\\\}";
            }
            else if (match.Groups[4].Success)
            {
                dyn.start++;
                dyn.length = 0;
                blocks.Add(dyn);
                return "{" + cmd + "{" + emptyPlaceholder + "}}";
            }
            else
            {
                blocks.Add(dyn);
                return cmd + "{" + match.Value + "}";
            }
        });

        {
            // keep things stable by capturing words
            original = m_wrapholder.Replace(original, @"{$1}");
            // sanitize invalid scripts
            original = m_scriptEscape.Replace(original, @"{$1}$2");
        }
        return original;
    }

    private string Group2WithSpaceElimination(Match match, Group g)
    {
        var cc = match.Length - g.Length - 1;
        var d = new Block()
        {
            index = blocks.Count,
            start = match.Index,
            length = g.Length + 1,
            lineSeparator = -1,
        };
        var str = "\\vlink[" + blocks.Count + "]{\\" + g.Value + "}";
        blocks.Add(d);
        for (int i = 0; i < cc; i++)
        {
            var d2 = new Block()
            {
                index = blocks.Count,
                start = match.Index + g.Length + 1 + i,
                length = 1,
                lineSeparator = -1,
            };
            str += "\\vlink[" + blocks.Count + "]{ }";
            blocks.Add(d2);
        }
        return str;
    }

    internal IEnumerable<Block> GetBlockMatches(int start, int length)
    {
        return blocks.Where((x) => x.start >= start && (x.end) <= (start + length));
    }

    internal Block GetBlockBefore(int pos)
    {
        Block b = new Block() { index = -1 };
        foreach (var i in blocks)
        {
            if (i.start > pos)
                break;
            b = i;
        }
        return b;
    }

    internal int GetBlockLineNumber(Block b)
    {
        var i = b.index;
        do
        {
            if (blocks[i].lineSeparator >= 0)
                return blocks[i].lineSeparator;
        } while (i-- >= 0);
        return 0;
    }

    internal Block GetNext(Block block)
    {
        if (block.index < blocks.Count - 1)
            return blocks[block.index + 1];
        else
            return block;
    }

    internal Block GetPrevious(Block block)
    {
        if (block.index > 0)
            return blocks[block.index - 1];
        else
            return block;
    }

    public int GetNearestPosition(Vector2 pos)
    {
        var param = tex.drawingParams;
        var lines = param.formulas;

        // scan y for relevant line to scan
        int linePos = -1, linePosInParam = -1;

        if (pos.y > param.size.y + param.offset.y)
            linePos = linePosInParam = 0;
        else
        {
            int iter = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].partOfPreviousLine == 0)
                    iter++;
                if (i == lines.Count - 1 || pos.y >= (lines[i].Y - lines[i].Depth) * param.scaleFactor)
                {
                    linePos = iter;
                    linePosInParam = i;
                    break;
                }
            }
        }

        // get any relevant info
        int wrapStart, wrapLength; // param.lines[] range
        int start = linePos == 0 ? 0 : -1, end = -1; // tex.text[] range

        param.GetLineRange(linePos, out wrapStart, out wrapLength);

        for (int i = 0; i < blocks.Count; i++)
        {
            var b = blocks[i];
            if (i == blocks.Count - 1)
                end = tex.text.Length;
            else if (b.lineSeparator < 0)
                continue;
            else if (b.lineSeparator == linePos - 1)
                start = b.start + 1;
            else if (b.lineSeparator == linePos)
            {
                end = b.start;
                break;
            }
        }

        // find nearest x
        var rects = tex.drawingContext.linkBoxRect;
        var line = param.GetInnerAreaOfLine(linePosInParam);
        if (wrapStart == linePosInParam && pos.x < line.x)
            return start;
        else if (wrapStart + wrapLength - 1 == linePosInParam && pos.x > line.xMax)
            return end;
        else
        {
            var res = 0;
            var dist = float.MaxValue;
            for (int i = start; i <= end;)
            {
                var b = GetBlockBefore(i);
                if (b.index < 0)
                {
                    i++;
                    continue;
                }

                var r = rects[b.index];
                var d = SqDistance(r, pos);

                if (d < dist)
                {
                    res = b.index;
                    dist = d;
                    if (d < 1e-3f)
                        break;
                }

                i = System.Math.Max(i + 1, b.end);
            }

            var resb = blocks[res];
            return resb.start + (pos.x > rects[res].center.x ? resb.length : 0);
        }
    }

    private static float SqDistance(Rect r, Vector2 p)
    {
        var c = r.center;
        var dx = System.Math.Max(System.Math.Abs(p.x - c.x) - r.width / 2, 0);
        var dy = System.Math.Max(System.Math.Abs(p.y - c.y) - r.height / 2, 0);
        return dx * dx + dy * dy * 9; // yeah assume vertical is "heavier" here
    }
}
