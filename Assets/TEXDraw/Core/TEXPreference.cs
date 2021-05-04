#if UNITY_EDITOR

using UnityEditor;
using System.IO;

#endif

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TexDrawLib
{
    public partial class TEXPreference : ScriptableObject
    {
        /// <summary>
        /// Main & Shared access to TEXDraw Preference
        /// </summary>
        static public TEXPreference main;

        static public void Initialize()
        {
#if UNITY_EDITOR
            if (!main)
            {
                //Get the Preference
                string[] targetData = AssetDatabase.FindAssets("t:TEXPreference");
                if (targetData.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(targetData[0]);
                    main = AssetDatabase.LoadAssetAtPath<TEXPreference>(path);
                    main.MainFolderPath = Path.GetDirectoryName(path);
                    // TEXDraw preference now put into resources files after 3.0
                    if (main.MainFolderPath.Contains("Resources"))
                        main.MainFolderPath = Path.GetDirectoryName(main.MainFolderPath);
                    if (targetData.Length > 1)
                        Debug.LogWarning("You have more than one TEXDraw preference, ensure that only one Preference exist in your Project");
                }
                else
                {
                    //Create New One
                    main = CreateInstance<TEXPreference>();
                    if (AssetDatabase.IsValidFolder(DefaultTexFolder))
                    {
                        AssetDatabase.CreateAsset(main, DefaultTexFolder + "/Resources/TEXDrawPreference.asset");
                        main.FirstInitialize(DefaultTexFolder);
                    }
                    else
                    {
                        //Find alternative path to the TEXPreference, that's it: Parent path of TEXPreference script.
                        string AlternativePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(main));
                        AlternativePath = Directory.GetParent(AlternativePath).Parent.FullName;
                        AssetDatabase.CreateAsset(main, AlternativePath + "/Resources/TEXDrawPreference.asset");
                        main.FirstInitialize(AlternativePath);
                    }
                }
            }
#else
             if (!main)
                    // The only thing that we can found is in the Resource folder
                    main = (TEXPreference)Resources.Load("TEXDrawPreference");
#endif
            // also init the neighborhood
            TEXConfiguration.Initialize();
        }

        // This editor only MainFolderPath is auto reloaded
        public string MainFolderPath = "Assets/TEXDraw";

        private const string DefaultTexFolder = "Assets/TEXDraw";

        public bool IncludeMathSlot = true;

        public int header_mathCount;
        public int header_userCount;

        /// Check if we are on importing process.
        /// This solve issues where TEXDraw component
        /// tries to render in the middle of importing process..
        public bool editorReloading = false;

        #region Runtime Utilities

        public Material defaultMaterial;

        /// <summary>
        /// List of fonts registered in TEXDraw
        /// </summary>
        [FormerlySerializedAs("fontData")]
        public TexAsset[] fonts;

        // Dictionaries for faster lookups
        [NonSerialized]
        public Dictionary<string, int> symbols = new Dictionary<string, int>();

        [NonSerialized]
        public Dictionary<char, int> charmaps = new Dictionary<char, int>();

        [NonSerialized]
        public Dictionary<string, TexAsset> fontnames = new Dictionary<string, TexAsset>();

        public TEXConfiguration preferences;

        public int[] glueTable = new int[100];

        public TexAsset GetFontByID(string id)
        {
            return fontnames[id, null];
        }

        public int GetFontIndexByID(string id)
        {
            TexAsset f;
            return fontnames.TryGetValue(id, out f) ? f.index : -1;
        }

        public TexChar GetChar(int font, char ch)
        {
            return fonts[font].GetChar(ch);
        }

        public TexChar GetChar(int hash)
        {
            if (hash < 0)
                return null;
            return fonts[hash >> 8].chars[hash % (1 << 8)];
        }

        public TexChar GetChar(string symbol)
        {
            return GetChar(symbols[symbol, -1]);
        }

        public int GetTypefaceFor(char ch)
        {
            int font;
            if (ch >= 'A' && ch <= 'Z') // char.IsUpper(ch)
                font = preferences.Typeface_Capitals;
            else if (ch >= 'a' && ch <= 'z') // char.IsLower(ch)
                font = preferences.Typeface_Small;
            else if (ch >= '0' && ch <= '9') // char.IsDigit(ch)
                font = preferences.Typeface_Number;
            else // 0x7F and above
                font = preferences.Typeface_Unicode;

            return font;
        }

        public bool IsCharAvailable(int font, char ch)
        {
            return fonts[font].GetChar(ch) != null;
        }

        public int GetGlue(CharType leftType, CharType rightType)
        {
            return glueTable[(int)leftType * 10 + (int)rightType];
        }

        static public int CharToHash(TexChar ch)
        {
            return ch.ToHash();
        }

        static public int CharToHash(int font, int ch)
        {
            return ch | font << 8;
        }

        [Obsolete("No more been used. Use the Dictionary instead")]
        static public int TranslateChar(int charIdx)
        {
            //An Integer Conversion from TEX-Character-Space (0-7F) to Actual-Character-Map (ASCII Latin-1)
            if (charIdx >= 0x0 && charIdx <= 0xf)
                return charIdx + 0xc0;
            if (charIdx == 0x10)
                return 0xb0;
            if (charIdx >= 0x11 && charIdx <= 0x16)
                return charIdx + (0xd1 - 0x11);
            if (charIdx == 0x17)
                return 0xb7;
            if (charIdx >= 0x18 && charIdx <= 0x1c)
                return charIdx + (0xd8 - 0x18);
            if (charIdx >= 0x1d && charIdx <= 0x1e)
                return charIdx + (0xb5 - 0x1d);
            if (charIdx == 0x1f)
                return 0xdf;
            if (charIdx == 0x20)
                return 0xef;
            if (charIdx >= 0x21 && charIdx <= 0x7e)
                return charIdx;
            if (charIdx == 0x7f)
                return 0xff;
            return 0;
        }

        #endregion
    }
}
