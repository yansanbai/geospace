using UnityEngine;

namespace TexDrawLib
{
    public class TEXConfiguration : ScriptableObject
    {
        static public TEXConfiguration main;

        public static void Initialize()
        {
            if (!main)
            {
                // The only thing that we can found is in the Resource folder
                if (TEXPreference.main)
                    main = TEXPreference.main.preferences;
                if (!main)
                {
                    main = TEXPreference.main.preferences = (TEXConfiguration)Resources.Load("TEXDrawConfiguration");
#if UNITY_EDITOR
                    if (!main)
                    {
                        main = TEXPreference.main.preferences = CreateInstance<TEXConfiguration>();
                        UnityEditor.AssetDatabase.CreateAsset(main, TEXPreference.main.MainFolderPath + "/Resources/TEXDrawConfiguration.asset");
                    }
                    UnityEditor.EditorUtility.SetDirty(TEXPreference.main);
#endif
                }
            }
        }

        [Header("Global Spaces")]
        [Range(0, 2), Tooltip("Globally accepted space letter width")]
        public float SpaceWidth = 0.35f;

        [Range(0, 2), Tooltip("Minimal line height")]
        public float LineHeight = 0.75f;

        [Range(0, 2), Tooltip("Minimal line depth")]
        public float LineDepth = 0.25f;

        [Range(0, 2), Tooltip("Space between lines")]
        public float LineSpace = 0.15f;

        [Range(0, 0.2f), Tooltip("Global ratio for Glue (kerning for each characters)")]
        public float GlueRatio = 0.033f;

        [Range(0, 0.2f), Tooltip("Additional padding amount for delimiters (so delimiters will looked as connected)")]
        public float ExtentPadding = 0.032f;

        [Range(-1f, 1.5f), Tooltip("Levitation amount for centered expression")]
        public float AxisHeight = 0.32f;

        [Header("Scale Factors")]
        [Range(0.1f, 1f), Tooltip("Size ratio for first-level depth of script")]
        public float ScriptFactor = 0.56f;

        [Range(0.1f, 1f), Tooltip("Size ratio for second-level depth of script")]
        public float NestedScriptFactor = 0.38f;

        [Range(-.5f, .5f), Tooltip("Additional value for determining minimum delimiter height")]
        public float DelimiterRecursiveOffset = 0.1f;

        [Header("Line and Strips")]
        [Range(0f, 0.5f), Tooltip("Thickness for all kinds of line")]
        public float LineThickness = 0.044f;

        [Range(0f, 2f), Tooltip("Negation Line Additional Width")]
        public float NegateMargin = 0.3f;

        [Range(0f, 1f), Tooltip("Double line margin")]
        public float DoubleNegateMargin = 0.08f;

        [Range(-1f, 1f), Tooltip("Upward offset for underlines")]
        public float UnderLineOffset = 0f;

        [Range(-1f, 1f), Tooltip("Upward offset for overrlines")]
        public float OverLineOffset = 0f;

        [Range(-1f, 1f), Tooltip("Upward offset for strike-through lines")]
        public float MiddleLineOffset = 0f;

        [Header("Expression Margins")]
        [Range(0f, 2f), Tooltip("Gap value for each separate cell")]
        public float MatrixMargin = 0.4f;

        [Range(0f, 2f), Tooltip("Additional padding for \\link expression")]
        public float LinkMargin = 0.4f;

        [Range(0f, 2f), Tooltip("Upward space for accents")]
        public float AccentMargin = 0.1f;

        [Range(0f, 2f), Tooltip("Horizontal space for extending horizontal width of fractions")]
        public float FractionMargin = 0.2f;

        [Range(0f, 2f), Tooltip("Horizontal space between rooftop and the base")]
        public float RootMargin = 0.1f;

        [Range(0f, 2f), Tooltip("Additional padding for \\bg expression")]
        public float BackdropMargin = 0.1f;

        [Header("Fraction Gaps")]
        [Range(0f, 2f), Tooltip("Vertical gap between numerator baseline and the line")]
        public float NumeratorShift = 0.33f;

        [Range(0f, 2f), Tooltip("Vertical gap between denominator baseline and the line")]
        public float DenominatorShift = 0.7f;

        [Range(0f, 2f), Tooltip("Numerator/denominator minimal distance to the line")]
        public float FractionGap = 0.3f;

        [Range(0f, 2f), Tooltip("Alternative minimal gap distance between numerator/denominator")]
        public float FractionGapNoLine = 0.3f;

        [Range(0f, 1f), Tooltip("Gap/shift factors for narrowed condition")]
        public float FractionNarrowFactor = 0.9f;

        [Header("Scripts Layout")]
        [Range(0f, 2f), Tooltip("Downward offset between superscript baseline and base's top bound")]
        public float SupDrop = 0.32f;

        [Range(0f, 2f), Tooltip("Downward offset between subscript baseline and base's low bound")]
        public float SubDrop = 0.25f;

        [Range(0f, 2f), Tooltip("Minimal allowed distance between subscript and superscript")]
        public float SupMin = 0.2f;

        [Range(0f, 2f), Tooltip("Alternative minimum distance in narrowed condition")]
        public float SupMinNarrow = 0.24f;

        [Range(0f, 2f), Tooltip("Minimum distance between base baseline and subscript baseline")]
        public float SubMinNoSup = 0.1f;

        [Range(0f, 2f), Tooltip("Alternative minimum distance between base and subscript baseline if superscript exist above it")]
        public float SubMinOnSup = 0.18f;

        [Header("Big Operator Layout")]
        [Range(0f, 2f), Tooltip("Additional height amount for any big operator")]
        public float BigOpMargin = 0.11f;

        [Range(0f, 2f), Tooltip("Distance between upper script baseline to base")]
        public float BigOpUpShift = 0.2f;

        [Range(0f, 2f), Tooltip("Minimum space between upper script to base")]
        public float BigOpUpperGap = 0.2f;

        [Range(0f, 2f), Tooltip("Distance between lower script baseline to base")]
        public float BigOpLowShift = 0.1f;

        [Range(0f, 2f), Tooltip("Minimum space between lower script to base")]
        public float BigOpLowerGap = 0.16f;

        [Header("Math Typefaces")]
        [Range(0, 31)]
        public int Typeface_Number;

        [Range(0, 31)]
        public int Typeface_Capitals;

        [Range(0, 31)]
        public int Typeface_Small;

        [Range(0, 31)]
        public int Typeface_Commands;

        [Range(0, 31)]
        public int Typeface_Text;

        [Range(0, 31)]
        public int Typeface_Unicode;

#if TEXDRAW_DEBUG
        [Header("Debugging Tools")]
        public bool Debug_HighlightBoxes;
#endif

        public int GetTypeface(TexCharKind kind)
        {
            switch (kind)
            {
                case TexCharKind.Numbers: return Typeface_Number;
                case TexCharKind.Capitals: return Typeface_Capitals;
                case TexCharKind.Small: return Typeface_Small;
                case TexCharKind.Commands: return Typeface_Commands;
                case TexCharKind.Text: return Typeface_Text;
                case TexCharKind.Unicode: return Typeface_Unicode;
                default: return 0;
            }
        }
    }
}
