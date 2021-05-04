using System.Collections.Generic;

namespace TexDrawLib
{
    public partial class TexFormulaParser
    {
        private Atom AttachScripts(TexFormula formula, string value, ref int position, Atom atom)
        {
            if (position == value.Length)
                return atom;
            if (value[position] == superScriptChar || value[position] == subScriptChar)
            {
                if (position == value.Length - 1)
                {
                    position++;
                    return atom;
                }
            }
            else
                return atom;

            TexFormula superscriptFormula = null;
            TexFormula subscriptFormula = null;

            //0: Undetermined; 1: Not Yet; 2: Yes, we are; 3: Yes, and make it smaller
            int markAsBig = 0;
            //0: In Beginning;1: We are in _;2: we are in ^
            int lastIsSuper = 0;

            while (position < value.Length)
            {
                var ch = value[position];
                if (ch == superScriptChar || ch == subScriptChar)
                {
                    if (++position < value.Length && (value[position] == ch))
                    {
                        markAsBig = 2;
                        if (++position < value.Length && (value[position] == ch))
                        {
                            markAsBig = 3;
                            position++;
                        }
                    }
                    bool v = ch == superScriptChar;
                    if ((v ? superscriptFormula : subscriptFormula) == null)
                        lastIsSuper = v ? 2 : 1;
                    continue;
                }
                else if (ch == rightGroupChar || (value[position - 1] != '^' && value[position - 1] != '_'))
                    break;
                if (lastIsSuper == 2)
                {
                    if (superscriptFormula == null)
                        superscriptFormula = ReadScript(formula, value, ref position);
                    else
                    {
                        position--;
                        superscriptFormula.RootAtom = AttachScripts(formula, value, ref position, superscriptFormula.RootAtom);
                    }
                }
                else if (lastIsSuper == 1)
                {
                    if (subscriptFormula == null)
                        subscriptFormula = ReadScript(formula, value, ref position);
                    else
                    {
                        position--;
                        subscriptFormula.RootAtom = AttachScripts(formula, value, ref position, subscriptFormula.RootAtom);
                    }
                }
                else
                    break;
            }
            if (superscriptFormula == null && subscriptFormula == null)
                return atom;

            // Check whether to return Big Operator or Scripts.
            if (atom != null && (atom.RightType == CharType.BigOperator || markAsBig >= 2))
                return BigOperatorAtom.Get(atom, subscriptFormula == null ? null : subscriptFormula.ExtractRoot(),
                    superscriptFormula == null ? null : superscriptFormula.ExtractRoot(), markAsBig == 3);
            else
                return ScriptsAtom.Get(atom, subscriptFormula == null ? null : subscriptFormula.ExtractRoot(),
                    superscriptFormula == null ? null : superscriptFormula.ExtractRoot());
        }

        private const string scriptCloseChars = " +-*/=()[]<>|.,;:`~\'\"?!@#$%&{}\\_^";
        private static readonly HashSet<char> scriptCloseCharsSet = new HashSet<char>(scriptCloseChars);

        public static string ReadScriptGroup(string value, ref int position)
        {
            if (position == value.Length)
                return string.Empty;

            var startPosition = position;
            var group = 0;
            position++;
            while (position < value.Length && !(group == 0 && isScriptCloseChar(value[position])))
            {
                if (value[position] == escapeChar)
                {
                    position++;
                    if (position == value.Length)
                    {
                        // Reached end of formula but group has not been closed.
                        return value.Substring(startPosition);
                    }
                }
                else if (value[position] == leftGroupChar)
                    group++;
                else if (isScriptCloseChar(value[position]))
                    group--;
                position++;
            }
            return value.Substring(startPosition, position - startPosition);
        }

        private TexFormula ReadScript(TexFormula formula, string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);

            if (position == value.Length)
                // abort
                return Parse(string.Empty);

            var ch = value[position];
            if (ch == leftGroupChar)
                return Parse(ReadGroup(value, ref position, leftGroupChar, rightGroupChar));
            else
                return Parse(ReadScriptGroup(value, ref position));
        }

        private static bool isScriptCloseChar(char c)
        {
            return scriptCloseCharsSet.Contains(c);
        }
    }
}
