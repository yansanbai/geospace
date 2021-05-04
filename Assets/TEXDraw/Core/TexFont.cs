using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TexDrawLib
{
    public class TexFont : TexAsset
    {
        public override TexAssetType type { get { return TexAssetType.Font; } }

        public override float LineHeight() { return asset.lineHeight; }

        public override Texture2D Texture() { return asset.material.mainTexture as Texture2D; }

        public Font asset;

        private static string[] chr = new string[0xFFFF];

        public CharacterInfo GenerateFont(char c, int size, FontStyle style)
        {
            CharacterInfo o; var a = asset;

            var s = chr[c] ?? (chr[c] = new string(c, 1));

            a.RequestCharactersInTexture(s, size, style);

            a.GetCharacterInfo(c, out o, size, style);

            return o;
        }

#if UNITY_EDITOR

        public override void ImportAsset(string path)
        {
            asset = AssetDatabase.LoadAssetAtPath<Font>(this.path = path);
        }

#endif
    }
}
