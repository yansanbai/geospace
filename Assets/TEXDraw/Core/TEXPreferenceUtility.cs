#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TexDrawLib
{
    public partial class TEXPreference : ScriptableObject
    {
        private void OnEnable()
        { PushToDictionaries(); }

        public bool PushToDictionaries()
        {
            int lastHash = 0;
            try
            {
                symbols.Clear();
                charmaps.Clear();
                fontnames.Clear();

                for (int i = 0; i < fonts.Length; i++)
                {
                    var font = fonts[i];
                    font.index = i;
                    font.ImportDictionary();
                    fontnames.Add(font.name, font);
                    var count = font.chars.Length;
                    for (int j = 0; j < count; j++)
                    {
                        TexChar c = font.chars[j];
                        c.index = j;
                        c.fontIndex = i;
                        lastHash = c.ToHash();

                        if (!string.IsNullOrEmpty(c.symbolName))
                            symbols.Add(c.symbolName, lastHash);
                        if (!string.IsNullOrEmpty(c.symbolAlt))
                            symbols.Add(c.symbolAlt, lastHash);
                        if (c.characterMap > 0)
                            charmaps.Add(TexChar.possibleCharMaps[c.characterMap], lastHash);
                    }
                }

                // now check charmaps full
                for (int i = 1; i < TexChar.possibleCharMaps.Length; i++)
                {
                    if (!charmaps.ContainsKey(TexChar.possibleCharMaps[i]))
                        Debug.LogWarning("TEXDraw: Charmap is missing: " + TexChar.possibleCharMaps[i].ToString());
                }
            }
            catch (System.Exception ex)
            {
                if (ex is System.ArgumentException)
                {
                    int pair;
                    try { pair = GetChar(GetChar(lastHash).symbolName).ToHash(); }
                    catch { return false; }
                    Debug.LogErrorFormat("TEXDraw: Duplicate Definitions Exist at {0:X3} and {1:X3}, Please fix it now.\nError: {2}", lastHash, pair, ex.Message);
                }
                else
                    Debug.LogErrorFormat("TEXDraw: Unknown Error at {0:X3}: {1}\n{2}\n\nStackTrace:\n{3}", lastHash, ex.GetType().Name, ex.Message, ex.StackTrace);
                return false;
            }
            return true;
        }

#if UNITY_EDITOR

        [ContextMenu("Rebuild Font Data")]
        public void Reload()
        {
            if (!EditorUtility.DisplayDialog("Confirm Reload",
                "Are you sure to rebuild the font data?",
                "OK", "No"))
                return;
            FirstInitialize(MainFolderPath);
        }

        [ContextMenu("Wipe All Data")]
        public void ResetWholeData()
        {
            var respond = EditorUtility.DisplayDialogComplex("Confirm Reset",
                "Do you really want to reset all symbol setups from beginning?\nThis is different than just reset as this deletes all generated stuff and regain data from beginning\n(WARNING: will erase anything in TexFontMetadata and can't be undone)",
                "YES", "Yes, don't pick up from XML", "Cancel");
            if (respond == 2)
                return;
            foreach (var f in fonts)
            {
                DestroyImmediate(f, true);
            }
            DestroyImmediate(preferences, true);
            FirstInitialize(MainFolderPath);
            preferences = TEXConfiguration.main;
            if (respond == 1)
                return;
            ResetWholeDataConfirmed();
        }

        private void ResetWholeDataConfirmed()
        {
            //var user = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexSymbolDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyXMLSymbols(this, false, user);
            //var math = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexMathDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyXMLSymbols(this, true, math);
            //var config = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TEXConfigurations.xml").text;
            //var preset = AssetDatabase.LoadAssetAtPath<TextAsset>(MainFolderPath + "/XMLs/TexFontDefinitions.xml").text;
            //TexImporterUtility.ReadLegacyPreferences(this, preset, config);
        }

        [ContextMenu("Transfer from Legacy XML Data")]
        public void ReloadLegacy()
        {
            var respond = EditorUtility.DisplayDialog("Confirm Transfer",
                "Are you sure you want to transfer symbols from XML data? Use this for maintaining projects which using TEXDraw prior to 3.0 or reset Symbols, Configs and Glue Matrix",
                "Yes", "Cancel");
            if (!respond)
                return;
            ResetWholeDataConfirmed();
        }

        public void FirstInitialize(string mainPath)
        {
            try
            {
                editorReloading = true;
                EditorUtility.DisplayProgressBar("Reloading", "Reading XML Files...", 0f);

                MainFolderPath = mainPath;

                TexImporterUtility.ReadFromResources(this);
                EditorUtility.DisplayProgressBar("Reloading", "Reading Configurations...", .95f);

                EditorUtility.DisplayProgressBar("Reloading", "Refreshing Instances...", .95f);

                PaintFontList();
                RebuildMaterial();
                PushToDictionaries();
                editorReloading = false;
                CallRedraw();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Failed to Rebuilding TEXDraw's Font Data");
            }
            EditorUtility.ClearProgressBar();
        }

        public void CallRedraw()
        {
            Component[] tex = FindObjectsOfType<Component>();
            for (int i = 0; i < tex.Length; i++)
            {
                if (tex[i] is ITEXDraw)
                {
                    ((ITEXDraw)tex[i]).SetTextDirty(true);
                    EditorUtility.SetDirty(tex[i]);
                }
            }
            SceneView.RepaintAll();
        }

        public string[] ConfigIDs;
        public string[] FontIDs;
        public GUIContent[] FontIDsGUI;
        public int[] FontIndexs;

        public void PaintFontList()
        {
            List<string> s = new List<string>();
            List<string> t = new List<string>();
            List<int> n = new List<int>();
            n.Add(-1);
            t.Add("-1 (Use Math Typefaces)");
            for (int i = 0; i < fonts.Length; i++)
            {
                t.Add(string.Format("{0} - {1}.ttf", i, fonts[i].id));
                n.Add(i);
                s.Add(fonts[i].id);
            }
            ConfigIDs = s.ToArray();
            FontIDs = t.ToArray();
            FontIndexs = n.ToArray();
            FontIDsGUI = t.ConvertAll(x => new GUIContent(x)).ToArray();
        }

        public Material[] watchedMaterial = new Material[0] { };

        public void RebuildMaterial()
        {
            EditorUtility.DisplayProgressBar("Updating...", "Updating TEXDraw materials", 0);
            if (!defaultMaterial)
            {
                defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(MainFolderPath + "/Resources/TEX-Default.mat");
                if (!defaultMaterial)
                    Debug.LogError("TEXDraw default Material Asset didn't found! please assign it manually.");
            }

            if (!watchedMaterial.Contains(defaultMaterial))
                ArrayUtility.Insert(ref watchedMaterial, 0, defaultMaterial);

            // Validate our lists first (no null and duplicates)
            watchedMaterial = watchedMaterial.Distinct().Where(x => x).ToArray();
            var texCount = fonts.Length;
            var count = watchedMaterial.Length;
            for (int i = 0; i < count; i++)
            {
                // Cosmetic
                EditorUtility.DisplayProgressBar("Updating...", "Updating TEXDraw materials (" + i + "/" + count + ")", i / (float)count);

                // Find best match sampler count in 'samples' shaders
                var mat = watchedMaterial[i];
                var type = mat.GetTag("TexMaterialType", false, "Null");
                var alts = mat.GetTag("TexMaterialAlts", false, "");

                if (type == "Null")
                {
                    Debug.LogWarningFormat("Material {0} is not using TEXDraw shaders. Removing it from Material Stacks", mat);
                    ArrayUtility.RemoveAt(ref watchedMaterial, i--);
                    continue;
                }

                if (alts.Length > 1 && alts[0] == '@')
                {
                    mat.shader = Shader.Find(alts.Substring(1)) ?? Shader.Find(alts.Substring(1) + "/Full");
                    alts = mat.GetTag("TexMaterialAlts", false, "");
                }
                if (alts.Length >= 1 && alts[0] != '@')
                {
                    var variants = TexCharPresets.CharsFromString(alts);
                    if (variants.Any(x => x >= texCount))
                    {
                        var target = (int)(variants.Where(x => x >= texCount).Min());
                        var name = mat.shader.name;
                        if (name.LastIndexOf("/Full") == name.Length - 5)
                            name = name.Substring(0, name.Length - 5);
                        var shaderTarget = string.Format("{0}/x{1:X} Samples", name, target);
                        var newShader = Shader.Find(shaderTarget);
                        if (newShader)
                            mat.shader = newShader;
                        else
                            Debug.LogWarningFormat("Shader {0} wasn't found, have you double check TexMaterialAlts?", shaderTarget);
                    }
                }
                // Now plug it on each data textures
                for (int j = 0; j < fonts.Length; j++)
                {
                    var propName = string.Format("_Font{0:X}", j);
                    var font = fonts[j];
                    if (!mat.HasProperty(propName))
                    {
                        Debug.LogWarningFormat("The shader {0} doesn't plug {1} or not declaring full fallback on TexMaterialAlts", mat.shader.name, propName);
                        break;
                    }
                    mat.SetTexture(propName, font.Texture());
                }

                // Now plug it on each data textures
                for (int j = fonts.Length; j < 31; j++)
                {
                    var propName = string.Format("_Font{0:X}", j);

                    if (mat.HasProperty(propName))
                    {
                        mat.SetTexture(propName, null);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }

#endif
    }
}
