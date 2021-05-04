using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
#endif

namespace TexDrawLib
{
    public abstract class TexAsset : ScriptableObject
    {
        /// <summary>
        /// type of this TexFont
        /// </summary>
        public abstract TexAssetType type { get; }

        /// <summary>
        /// Id (typically the TexFont name)
        /// </summary>
        public string id;

        /// <summary>
        /// Index in TEXPreference
        /// </summary>
        public int index;

        /// <summary>
        /// Editor configured import catalog
        /// </summary>
        [FormerlySerializedAs("importCatalog")]
        public string catalogRaw = "";

        /// <summary>
        /// Editor path to the asset
        /// </summary>
        [FormerlySerializedAs("assetPath")]
        public string path;

        /// <summary>
        /// List of characters that configurable, based on import catalog
        /// </summary>
        [FormerlySerializedAs("parsedCatalogs")]
        public char[] catalog = new char[0];

        public TexChar[] chars = new TexChar[0];

        protected Dictionary<char, TexChar> indexes = new Dictionary<char, TexChar>();

        // Also called by TexPreference's OnEnable
        public virtual void ImportDictionary()
        {
            indexes.Clear();
            int i = 0;
            foreach (var item in chars)
            {
                // these three are not serialized
                item.characterIndex = catalog[i];
                item.index = i;
                item.fontIndex = index;
                indexes[item.characterIndex] = item;
                i++;
            }
        }

        public TexChar GetChar(char c)
        {
            TexChar ch;
            return indexes.TryGetValue(c, out ch) ? ch : null;
        }

        public abstract float LineHeight();

        public abstract Texture2D Texture();

#if UNITY_EDITOR

        [ContextMenu("Export as JSON")]
        public void Export()
        {
            TEXPreference.Initialize();
            var path = TEXPreference.main.MainFolderPath + "Core/Editor/Resources/" + id + ".json";
            var json = JsonUtility.ToJson(this);

            Directory.CreateDirectory(path);
            File.WriteAllText(path, json);
            Debug.Log("Successfully written to " + path);
        }

        public void ImportCatalog(string raw)
        {
            catalogRaw = string.IsNullOrEmpty(raw) ? TexCharPresets.legacyChars : raw;
            catalog = TexCharPresets.CharsFromString(catalogRaw);
        }

        public void ImportCharacters(string newcatalog)
        {
            // map from old (existing catalog) to newer one.
            // preserve data from each characterindex

            ImportDictionary();

            catalogRaw = newcatalog;

            catalog = TexCharPresets.CharsFromString(newcatalog);

            var old = chars; chars = new TexChar[catalog.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                var cc = catalog[i];
                (chars[i] = (old.FirstOrDefault(x => x.characterIndex == cc) ?? new TexChar()
                {
                    characterIndex = cc,
                    fontIndex = index,
                })).index = i;
            }
        }

        public abstract void ImportAsset(string path);

#endif
    }
}
