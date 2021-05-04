#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TexDrawLib
{
    /// Contains some utility codes to manage the automatic importing processes.
    public class TexImporterUtility
    {
        public static void ReadFromResources(TEXPreference pref)
        {
            var fontList = new List<TexAsset>();
            if (!AssetDatabase.IsValidFolder(pref.MainFolderPath + "/Resources/TexFontMetaData"))
                AssetDatabase.CreateFolder(pref.MainFolderPath + "/Resources", "TexFontMetaData");
            LoadPrimaryDefinitionSubset<TexFont>(pref, fontList, pref.MainFolderPath + "/Fonts/Math", "t:Font", TexAssetType.Font, 0);
            pref.header_mathCount = fontList.Count;
            LoadPrimaryDefinitionSubset<TexFont>(pref, fontList, pref.MainFolderPath + "/Fonts/User", "t:Font", TexAssetType.Font, 1);
            pref.header_userCount = fontList.Count;
            LoadPrimaryDefinitionSubset<TexSprite>(pref, fontList, pref.MainFolderPath + "/Fonts/Sprites", "t:Sprite", TexAssetType.Sprite, 2);
            EditorUtility.DisplayProgressBar("Reloading", "Preparing Stuff...", .93f);

            pref.fonts = fontList.ToArray();
        }

        private const string resourceFontMetaPath = "/Resources/TexFontMetaData/";

        private static void LoadPrimaryDefinitionSubset<T>(TEXPreference pref, List<TexAsset> fontList, string folderPath, string typeStr, TexAssetType typeEnum, int mode) where T : TexAsset
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;
            string[] customF = AssetDatabase.FindAssets(typeStr, new string[] { folderPath });
            for (int i = 0; i < customF.Length; i++)
            {
                if (fontList.Count >= 31)
                {
                    Debug.LogWarning("Font/Sprite database count are beyond 31, ignoring any assets after " + fontList[fontList.Count - 1].id);
                    break;
                }
                string realPath = AssetDatabase.GUIDToAssetPath(customF[i]);
                string id = Path.GetFileNameWithoutExtension(realPath).ToLower();

                if (fontList.Any((x) => x.id == id)) continue;

                if (!isNameValid(id))
                {
                    // We show this for information purpose, since *-Regular or *_ is very common mistake
                    // We are not showing this for frequent update, since this behavior is 'intended' for giving an alternative styling
                    if (id.Contains("-Regular") || id.Substring(id.Length - 1) == "_")
                        Debug.LogWarning("File " + id + " is ignored since it has invalid character(s) in its name");
                    continue;
                }

                UpdateProgress(mode, id, i, customF.Length);

                string json = null;
                var metaPath = pref.MainFolderPath + resourceFontMetaPath + id + ".asset";
                TexAsset metadata = AssetDatabase.LoadAssetAtPath<T>(metaPath);

#if TEXDRAW_TMP
                string sdfPath = pref.MainFolderPath + "/Fonts/TMPro/" + id + ".asset";
                if (typeEnum == TexAssetType.Font && File.Exists(sdfPath))
                {
                    metadata = AssetDatabase.LoadAssetAtPath<TexFontSigned>(metaPath);
                    if (!metadata)
                    {
                        metadata = CreateAndRecover<TexFontSigned>(metaPath);
                        ((TexFontSigned)metadata).rawpath = realPath;
                        AssetDatabase.CreateAsset(metadata, metaPath);
                    }
                    realPath = sdfPath;
                }
#endif
                if (!metadata)
                {
                    metadata = CreateAndRecover<T>(metaPath);
                    AssetDatabase.CreateAsset(metadata, metaPath);
                }
                else
                    json = JsonUtility.ToJson(metadata);

                metadata.id = id;
                metadata.index = fontList.Count;
                metadata.ImportAsset(realPath);
                metadata.ImportDictionary();

                if (json != JsonUtility.ToJson(metadata))
                    // this is necessary to avoid messing with Git version control
                    EditorUtility.SetDirty(metadata);

                fontList.Add(metadata);
            }
        }

        private static TexAsset CreateAndRecover<T>(string metaPath) where T : TexAsset
        {
            TexAsset metadata = ScriptableObject.CreateInstance<T>();
            {
                // it may saved in another format. try to recover.
                var mt2 = AssetDatabase.LoadAssetAtPath<TexAsset>(metaPath);
                if (mt2)
                {
                    EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(mt2), metadata);
                    AssetDatabase.DeleteAsset(metaPath);
                }
                else
                    metadata.ImportCharacters(TexCharPresets.legacyChars);
            }
            return metadata;
        }

        private static void UpdateProgress(int phase, string name, int idx, int total)
        {
            var prog = idx / (float)total;
            prog = phase * 0.3f + (prog * 0.3f);
            EditorUtility.DisplayProgressBar("Reloading", "Reading " + name + "...", prog);
        }

        private static bool isNameValid(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsLetter(name[i]))
                    continue;
                else
                    return false;
            }
            return true;
        }

        //Converting a hash from XML file to Our Font map,
        private static int SyncHash(int origHash, List<int> syncMap)
        {
            if (origHash == -1)
                return -1;
            if (syncMap[origHash >> 8] == -1)
                return -1;
            return (syncMap[origHash >> 8] << 8) + (origHash % 128);
        }
    }
}

#endif
