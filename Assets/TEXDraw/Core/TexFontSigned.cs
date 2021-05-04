using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if TEXDRAW_TMP
using TMPro;
#endif

namespace TexDrawLib
{
    public class TexFontSigned : TexAsset
    {
        public override TexAssetType type { get { return TexAssetType.FontSigned; } }

        public string rawpath;

#if TEXDRAW_TMP

        public override float LineHeight() { return asset.fontInfo.LineHeight / asset.fontInfo.PointSize; }

        public override Texture2D Texture() { return asset.atlas; }

        public TMP_FontAsset asset;

        protected Dictionary<char, SpriteMetrics> assetmetrices = new Dictionary<char, SpriteMetrics>();

        public SpriteMetrics GenerateMetric(char c)
        {
            return assetmetrices.GetValue(c);
        }

        public override void ImportDictionary()
        {
            base.ImportDictionary();

            if (!asset) return;

            // sanitize input

            assetmetrices.Clear();

            var info = asset.fontInfo;

            var padding = asset.fontInfo.Padding;

            for (int i = 0; i < catalog.Length; i++)
            {
                if (!asset.characterDictionary.ContainsKey(catalog[i]))
                {
                    assetmetrices[catalog[i]] = new SpriteMetrics();
                    continue;
                }

                TMP_Glyph c = asset.characterDictionary[catalog[i]];

                var factor = c.scale / info.PointSize;

                assetmetrices[catalog[i]] = new SpriteMetrics()
                {
                    size = new Vector4()
                    {
                        x = (-c.xOffset + padding) * factor,
                        y = (c.height - c.yOffset + padding) * factor,
                        z = (c.width + c.xOffset + padding) * factor,
                        w = (c.yOffset + padding) * factor,
                    },
                    advance = c.xAdvance * factor,
                    uv = new Rect()
                    {
                        x = (c.x - padding) / info.AtlasWidth,
                        y = 1 - (c.y + c.height + padding) / info.AtlasHeight,
                        width = (c.width + 2 * padding) / info.AtlasWidth,
                        height = (c.height + 2 * padding) / info.AtlasHeight
                    }
                };
            }
        }
#else

        public override float LineHeight() { return 0; }

        public override Texture2D Texture() { return null; }

#endif

#if UNITY_EDITOR

        public override void ImportAsset(string path)
        {
#if TEXDRAW_TMP
            asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
#endif
        }

#endif
    }
}
