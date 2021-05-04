namespace TexDrawLib
{
    public enum CharType
    {
        Ordinary = 0,
        Geometry = 1,
        Operator = 2,
        Relation = 3,
        Arrow = 4,
        OpenDelimiter = 5,
        CloseDelimiter = 6,
        BigOperator = 7,
        Accent = 9,
    }

    public static class CharTypeInternal
    {
        static public readonly CharType Invalid = (CharType)(-1);
        static public readonly CharType Inner = (CharType)8;
    }

    public enum ExtensionType
    {
        Repeat = 0,
        Top = 1,
        Bottom = 2,
        Mid = 3
    }

    public enum TexAlignment
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 4
    }

    public enum TexEnvironment
    {
        Display = 0,

        //DisplayCramped = 1
        Text = 2,

        //TextCramped = 3
        Script = 4,

        //ScriptCramped = 5
        ScriptScript = 6,

        //ScriptScriptCramped = 7
    }

    public enum TexCharKind
    {
        None = -1,
        Numbers = 0,
        Capitals = 1,
        Small = 2,
        Commands = 3,
        Text = 4,
        Unicode = 5
    }

    public enum StrutPolicy
    {
        Misc = 0,
        BlankSpace = 1,
        Glue = 2,
        EmptyLine = 3,
        TabSpace = 4,    // Not used, however
        MetaBlock = 5,
    }

    public enum TexAssetType
    {
        Font = 0,
        Sprite = 1,
        FontSigned = 2
    }

    public enum Wrapping
    {
        NoWrap = 0,
        LetterWrap = 1,
        WordWrap = 2,
        WordWrapJustified = 3,
#if TEXDRAW_REVERSEDWRAPPING
        LetterWrapReversed = 4,
        WordWrapReversed = 5,
        WordWrapReversedJustified = 6,
#endif
    }

    public enum Fitting
    {
        Off = 0,
        DownScale = 1,
        RectSize = 2,
        HeightOnly = 3,
        Scale = 4,
        BestFit = 5
    }

    public enum Filling
    {
        None = 0,
        Rectangle = 1,
        WholeText = 2,
        WholeTextSquared = 3,
        PerLine = 4,

        //PerWord = 5, //Not yet ready
        PerCharacter = 6,

        PerCharacterSquared = 7,
        LocalContinous = 8,
        WorldContinous = 9
    }

    public enum Effects
    {
        Shadow = 0,
        Outline = 1
    }
}
