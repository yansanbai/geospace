using UnityEngine;

namespace TexDrawLib
{
    public partial class TexFormulaParser
    {
        // Special characters for parsing
        private const string extraSpace = "w";

        private const string extraSpaceSoft = "s";
        private const char escapeChar = '\\';

        private const char leftGroupChar = '{';
        private const char rightGroupChar = '}';
        private const char leftBracketChar = '[';
        private const char rightBracketChar = ']';

        private const char subScriptChar = '_';
        private const char superScriptChar = '^';

        public static readonly char[] preservedCharacters = { '\\', '{', '}', '_', '^' };

        private static bool IsSymbol(char c)
        {
            return !((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        }

        public static bool IsParserReserved(char c)
        {
            return (c == escapeChar) || (c == leftGroupChar) ||
            (c == rightGroupChar) || (c == subScriptChar) || (c == superScriptChar);
        }

        private static bool IsWhiteSpace(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\n';
        }

        public TexFormulaParser() { }

        // the secret sauce begin here
        public TexFormula Parse(string value)
        {
            if (value == null) return null;
            var formula = ObjPool<TexFormula>.Get();
            var position = 0;
            while (position < value.Length)
            {
                char ch = value[position];
                if (ch == '\r' || ch == '\0' || ch == '\x200c')
                    position++; // Skip chars that have zero width length
                else if (IsWhiteSpace(ch))
                {
                    formula.Add(AttachScripts(formula, value, ref position, SpaceAtom.Get()));
                    position++;
                }
                else if (ch == escapeChar)
                {
                    ProcessEscapeSequence(formula, value, ref position);
                }
                else if (ch == leftGroupChar)
                {
                    formula.Add(AttachScripts(formula, value, ref position, Parse(ReadGroup(value,
                        ref position, leftGroupChar, rightGroupChar)).ExtractRoot()));
                }
                else if (ch == rightGroupChar)
                {
                    position++;
                }
                else if (ch == superScriptChar || ch == subScriptChar)
                {
                    formula.Add(AttachScripts(formula, value, ref position, SpaceAtom.Get(0, TexUtility.spaceHeight, 0)));
                }
                else
                {
                    var scriptsAtom = AttachScripts(formula, value, ref position,
                            ConvertCharacter(value, ref position, ch));
                    formula.Add(scriptsAtom);
                }
            }

            if (formula.RootAtom == null && formula.AttachedMetaRenderer == null)
                formula.Add(SpaceAtom.Get(false)); // a blank paragraph

            return formula;
        }

        //StringBuilder builderGroup = new StringBuilder();

        public static string ReadGroup(string value, ref int position, char openChar, char closeChar)
        {
            if (position == value.Length)
                return string.Empty;

            var readCloseGroup = true;
            if (value[position] != openChar)
                readCloseGroup = false;
            else
                position++;
            var startPosition = position;
            var group = 0;
            while (position < value.Length && !(value[position] == closeChar && group == 0))
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
                else if (value[position] == openChar)
                    group++;
                else if (value[position] == closeChar)
                    group--;
                position++;
            }

            if (position == value.Length)
            {
                return value.Substring(startPosition);
            }
            if (readCloseGroup)
            {
                position++;
                return value.Substring(startPosition, position - startPosition - 1);
            }
            return value.Substring(startPosition, position - startPosition);
        }

        public static string LookForAWord(string value, ref int position)
        {
            var startPosition = position;
            while (position < value.Length)
            {
                var ch = value[position];
                var isEnd = position == value.Length - 1;
                if (!char.IsLetter(ch) || isEnd)
                {
                    // Escape sequence has ended.
                    if (char.IsLetter(ch))
                        position++;
                    break;
                }
                position++;
            }
            return value.Substring(startPosition, position - startPosition);
        }

        private void ProcessEscapeSequence(TexFormula formula, string value, ref int position)
        {
            position++;

            var command = LookForAWord(value, ref position);

            // Check if there's no command
            if (command.Length == 0)
            {
                if (position < value.Length)
                {
                    var nextChar = value[position];
                    if (IsParserReserved(nextChar))
                        formula.Add(ConvertCharacter(value, ref position, nextChar));
                }
                return;
            }

            //Check if the command registered in Commands
            if (isCommandRegistered(command))
            {
                formula.Add(AttachScripts(formula, value, ref position,
                    ProcessCommand(formula, value, ref position, command)));
                return;
            }

            //Check if the command registered in FontID
            var fontID = TEXPreference.main.GetFontIndexByID(command);

            if (fontID != -1)
            {
                SkipWhiteSpace(value, ref position);
                string style = "";
                if (position < value.Length)
                {
                    style = ReadInsideBracket(value, ref position);
                }
                formula.Add(ParseFontStyle(value, ref position, fontID,
                    ParseStylePrefix(style)), true);
                return;
            }

            SymbolAtom symbolAtom = SymbolAtom.GetAtom(command);

            //Check if the command registered in Symbol database

            if (symbolAtom != null)
            {
                // Symbol was found.
                if (symbolAtom.RightType == CharType.Accent && formula.RootAtom != null)
                {
                    //Accent is Found
                    Atom baseAtom = formula.RootAtom;
                    if (baseAtom is RowAtom)
                    {
                        var row = (RowAtom)baseAtom;
                        baseAtom = MatrixAtom.Last(row.Elements);
                        row.Elements.RemoveAt(row.Elements.Count - 1);
                    }
                    else
                    {
                        formula.RootAtom = null;
                    }

                    formula.Add(AttachScripts(formula, value, ref position, AccentedAtom.Get(baseAtom, symbolAtom)));
                }
                else
                    formula.Add(AttachScripts(formula, value, ref position, symbolAtom));
                return;
            }

            //No lucks, now ...

            {
                // Command aren't defined, use it as command text style
                RowAtom row = RowAtom.Get();
                for (int i = 0; i < command.Length; i++)
                {
                    var charAtom = CharAtom.Get(command[i],
                                    TEXConfiguration.main.Typeface_Commands);
                    row.Add(charAtom);
                }
                formula.Add(AttachScripts(formula, value, ref position, row));
            }
        }

        private Atom ConvertCharacter(string value, ref int position, char character)
        {
            position++;
            var font = TexContext.Font.value;

            if (font == -1)
            {
                // Character is symbol (and math).
                var charIdx = TEXPreference.main.charmaps[character, -1];
                if (charIdx >= 0)
                    return SymbolAtom.Get(TEXPreference.main.GetChar(charIdx), character);
                else
                    return CharAtom.Get(character, -1);
            }
            else
            {
                return CharAtom.Get(character, font);
            }
        }

        static public void SkipWhiteSpace(string value, ref int position)
        {
            while (position < value.Length && IsWhiteSpace(value[position]))
                position++;
        }

        private Atom InsertAttribute(Atom atom, Atom begin, Atom end)
        {
            if (!(atom is RowAtom))
                atom = RowAtom.Get(atom);
            var row = (RowAtom)atom;
            row.Add(end);
            row.Elements.Insert(0, begin);
            return row;
        }

        private FontStyle ParseStylePrefix(string prefix)
        {
            switch (prefix)
            {
                case "n":
                    return FontStyle.Normal;
                case "b":
                    return FontStyle.Bold;
                case "i":
                    return FontStyle.Italic;
                case "bi":
                    return FontStyle.BoldAndItalic;
                default:
                    return TexUtility.FontStyleDefault;
            }
        }
    }
}
