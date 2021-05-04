using System;

namespace TexDrawLib
{
    [System.Serializable]
    public class TexChar
    {
        /// <summary>
        /// The index relative on containing font
        /// </summary>
        [NonSerialized]
        public int index;

        /// <summary>
        /// The Unicode index that this character belongs to. Most relevant for fonts.
        /// NOTE: DO NOT confuse with index. All APIs denote 'char' for characterIndex.
        /// </summary>
        [NonSerialized]
        public char characterIndex;

        /// <summary>
        /// The font index that contaning this instance. Used for backreference
        /// </summary>
        [NonSerialized]
        public int fontIndex;

        /// <summary>
        /// The type of Character. Most useful for differentiate kernings
        /// </summary>
        public CharType type = CharType.Ordinary;

        public TexAsset font
        {
            get { return TEXPreference.main.fonts[fontIndex]; }
            set { fontIndex = value.index; }
        }

        #region Extension Character Database

        //Is Larger (Similar) Character Exist ?, Then reference to it
        public int nextLargerHash = -1;

        public bool nextLargerExist { get { return nextLargerHash > -1; } }

        public TexChar nextLarger
        {
            get { return TEXPreference.main.GetChar(nextLargerHash); }
            set { nextLargerHash = value != null ? value.ToHash() : -1; }
        }

        //Extension Setup
        public bool extensionExist = false;

        public bool extensionHorizontal = false;

        public int extentTopHash = -1;

        public TexChar extentTop
        {
            get { return TEXPreference.main.GetChar(extentTopHash); }
            set { extentTopHash = value != null ? value.ToHash() : -1; }
        }

        public int extentMiddleHash = -1;

        public TexChar extentMiddle
        {
            get { return TEXPreference.main.GetChar(extentMiddleHash); }
            set { extentMiddleHash = value != null ? value.ToHash() : -1; }
        }

        public int extentBottomHash = -1;

        public TexChar extentBottom
        {
            get { return TEXPreference.main.GetChar(extentBottomHash); }
            set { extentBottomHash = value != null ? value.ToHash() : -1; }
        }

        public int extentRepeatHash = -1;

        public TexChar extentRepeat
        {
            get { return TEXPreference.main.GetChar(extentRepeatHash); }
            set { extentRepeatHash = value != null ? value.ToHash() : -1; }
        }

        #endregion

        public int ToHash()
        {
            return index | (fontIndex << 8);
        }

        public string symbolName;
        public string symbolAlt;
        public int characterMap = 0;
        public const string possibleCharMaps = " +-*/=()[]<>|.,;:`~\'\"?!@#$%&{}\\_^";

        public TexChar()
        {
        }

        //public TexChar(TexFont Font, int Index, char CharIndex, bool Supported, float scale)
        //{
        //    fontIndex = Font.index;
        //    index = Index;
        //    characterIndex = CharIndex;
        //    supported = Supported;
        //    extentTopHash = -1;
        //    extentMiddleHash = -1;
        //    extentBottomHash = -1;
        //    extentRepeatHash = -1;
        //    if (supported) {
        //        if (Font.type == TexFontType.Font) {
        //            CharacterInfo c = getCharInfo(Font.Font_Asset, CharIndex);
        //            UpdateGlyph(Font.Font_Asset, c);
        //        } else {
        //            depth = 0;
        //            height = scale;
        //            bearing = 0;
        //            italic = scale;
        //            width = scale;
        //        }
        //    }
        //}

        //public void UpdateGlyph(Font font, CharacterInfo c)
        //{
        //    font_reqGlyphSize = c.size == 0 ? font.fontSize : c.size;
        //    float ratio = font_reqGlyphSize;
        //    depth = -c.minY / ratio;
        //    height = c.maxY / ratio;
        //    bearing = -c.minX / ratio;
        //    italic = c.maxX / ratio;
        //    width = c.advance / ratio;

        //}

        //static CharacterInfo getCharInfo(Font font, char ch)
        //{
        //    string s = new string(ch, 1);
        //    font.RequestCharactersInTexture(s);
        //    CharacterInfo o;
        //    if (font.GetCharacterInfo(ch, out o))
        //        return o;
        //    else
        //        return new CharacterInfo();
        //}

        public CharBox GetMetric()
        {
            return CharBox.Get(this);
        }

        private static CharBox[] _chr = new CharBox[4]; // reusable as long as in single thread

        public CharBox[] GetExtentMetrics()
        {
            var metric = _chr;
            metric[0] = extentTopHash >= 0 ? extentTop.GetMetric() : null;
            metric[1] = extentMiddleHash >= 0 ? extentMiddle.GetMetric() : null;
            metric[2] = extentBottomHash >= 0 ? extentBottom.GetMetric() : null;
            metric[3] = extentRepeatHash >= 0 ? extentRepeat.GetMetric() : null;

            return metric;
        }
    }
}
