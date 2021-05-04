#if TEXDRAW_TMP

using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class TexTMPImporter
    {
        private static string[] FontResolutionLabels = { "16", "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private static int[] FontAtlasResolutions = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        public static int _bufferWidth = 512;
        public static int _bufferHeight = 256;
        public static int _padding = 2;
        public static FaceStyles _style = FaceStyles.Normal;
        public static RenderModes _render = RenderModes.DistanceField16;
        public static float _styleStrokeSize = 2;
        public static bool _optimized = false;

        private static FT_FaceInfo _faceInfo = new FT_FaceInfo();
        private static FT_GlyphInfo[] _glyphsInfo;
        private static Texture2D _atlasInfo;
        private static byte[] _buffers;
        private static int[] m_kerningSet;

        public static bool onRendering;
        /// <summary>0 = No, 1 = Yes, and overwrite, 2 = Yes, but no overwrite, 3 = Aborting</summary>
        public static int onRenderingBatch;
        public static bool isRenderingBatchFinished;
        public static bool hasRendered;
        public static int lastError;

        static void CheckErr(int error)
        {
            if (error != 0 && error != 99)
                throw new Exception("ERROR: " + error.ToString());
        }

        public static void DoBatchRendering()
        {
            var fonts = TEXPreference.main.fonts;

            ThreadPool.QueueUserWorkItem(SomeTask =>
            {
                int i = 0;
                for (; i < fonts.Length; i++)
                {
                    var f = fonts[i];
                    if (onRenderingBatch == 2 && f.type == TexAssetType.FontSigned)
                        continue;
                    else if (f.type == TexAssetType.Sprite)
                        continue;
                    else if (onRenderingBatch == 3)
                        break;
                    isRenderingBatchFinished = false;

                    EditorApplication.delayCall += delegate ()
                    {
                        CreateSDFAsset(f);
                    };

                    while (!isRenderingBatchFinished)
                    {
                        // Wait for a while
                        Thread.Sleep(500);
                    }
                }
                if (i < fonts.Length)
                    Debug.LogWarningFormat("Batch SDF Rendering was cancelled before {0}.ttf", fonts[i].id);
                else
                {
                    EditorApplication.delayCall += delegate ()
                    {
                        TEXPreference.main.FirstInitialize(TEXPreference.main.MainFolderPath);
                    };
                    Debug.Log("All TEXDraw fonts has been successfully rendered.");
                }

                onRenderingBatch = 0;
            });
        }

        public static void CreateSDFAsset(TexAsset asset)
        {
            switch (asset.type)
            {
                case TexAssetType.Font:
                    var font = ((TexFont)asset).asset;
                    CreateSDFAsset(asset, font, AssetDatabase.GetAssetPath(font));
                    break;
                case TexAssetType.Sprite:
                    // WTF?
                    break;
                case TexAssetType.FontSigned:
                    var path = ((TexFontSigned)asset).rawpath;
                    CreateSDFAsset(asset, AssetDatabase.LoadAssetAtPath<Font>(path), path);
                    break;
            }
        }

        public static void CreateSDFAsset(TexAsset asset, Font font, string fontPath)
        {
            // initialize
            CheckErr(TMPro_FontPlugin.Initialize_FontEngine());
            CheckErr(TMPro_FontPlugin.Load_TrueType_Font(fontPath));
            CheckErr(TMPro_FontPlugin.FT_Size_Font(72));

            _buffers = new byte[_bufferWidth * _bufferHeight];
            var charSet = new int[asset.chars.Length];
            _glyphsInfo = new FT_GlyphInfo[asset.chars.Length];
            _atlasInfo = null;

            for (int i = 0; i < asset.chars.Length; i++)
            {
                charSet[i] = asset.chars[i].characterIndex;
            }

            float strokeSize = _styleStrokeSize;
            if (_render == RenderModes.DistanceField16) strokeSize = _styleStrokeSize * 16;
            if (_render == RenderModes.DistanceField32) strokeSize = _styleStrokeSize * 32;

            ThreadPool.QueueUserWorkItem(SomeTask =>
            {
                try
                {
                    onRendering = true;

                    CheckErr(TMPro_FontPlugin.Render_Characters(_buffers, _bufferWidth, _bufferHeight, _padding,
                        charSet, charSet.Length, _style, strokeSize, true, _render, _optimized ? 4 : 0,
                        ref _faceInfo, _glyphsInfo));

                    EditorApplication.delayCall += delegate ()
                    {
                        try
                        {
                            OnFinished(Convert(asset));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                            Debug.LogWarningFormat("Failed to create SDF File for {0}.ttf. Consider delete and rerender again", asset.id);
                        }
                        finally
                        {
                            hasRendered = false;
                            onRendering = false;
                            if (onRenderingBatch > 0)
                                isRenderingBatchFinished = true;
                        }
                    };
                    Debug.LogFormat("Font Rendering of {0}.ttf has completed.", asset.id);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogWarningFormat("Failed to create Render {0}.ttf. Consider rerender again", asset.id);
                }
                finally
                {
                    onRendering = false;
                }
            });
        }

        static TexFontSigned Convert(TexAsset asset)
        {
            if (asset.type == TexAssetType.FontSigned)
                return (TexFontSigned)asset;
            else
            {
                var n = ScriptableObject.CreateInstance<TexFontSigned>();
                asset.ImportDictionary();
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(asset), n);
                n.rawpath = AssetDatabase.GetAssetPath(((TexFont)asset).asset);
                var main = TEXPreference.main;
                main.fontnames[asset.id] = main.fonts[Array.IndexOf(main.fonts, asset)] = n;
                AssetDatabase.CreateAsset(n, AssetDatabase.GetAssetPath(asset));
                return n;
            }
        }

        static TexFont Deconvert(TexFontSigned asset)
        {
            var n = ScriptableObject.CreateInstance<TexFont>();
            asset.ImportDictionary();
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(asset), n);
            n.ImportAsset(asset.rawpath);
            var main = TEXPreference.main;
            main.fontnames[asset.id] = main.fonts[Array.IndexOf(main.fonts, asset)] = n;
            AssetDatabase.CreateAsset(n, AssetDatabase.GetAssetPath(asset));
            n.ImportDictionary();
            return n;
        }

        static void OnFinished(TexFontSigned font)
        {
            var sdfPath = TEXPreference.main.MainFolderPath + "/Fonts/TMPro/" + font.id + ".asset";

            TMP_FontAsset asset;

            if (!(asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath)))
            {
                // It doesn't exist (or invalid), so create new
                asset = font.asset = ScriptableObject.CreateInstance<TMP_FontAsset>();
                if (!AssetDatabase.IsValidFolder(TEXPreference.main.MainFolderPath + "/Fonts/TMPro"))
                    AssetDatabase.CreateFolder(TEXPreference.main.MainFolderPath + "/Fonts", "TMPro");
                AssetDatabase.CreateAsset(asset, sdfPath);
            }
            asset.fontAssetType = _render >= RenderModes.DistanceField16 ? TMP_FontAsset.FontAssetTypes.SDF : TMP_FontAsset.FontAssetTypes.Bitmap;
            FaceInfo face = GetFaceInfo(_faceInfo, 1);
            asset.AddFaceInfo(face);

            _atlasInfo = new Texture2D(_bufferWidth, _bufferHeight, TextureFormat.Alpha8, false, true);
            var _buffer32 = Array.ConvertAll(_buffers, x => new Color32(x, x, x, x));
            _atlasInfo.SetPixels32(_buffer32);
            _atlasInfo.Apply(false, true);

            // Add GlyphInfo[] to Font Asset
            TMP_Glyph[] glyphs = GetGlyphInfo(_glyphsInfo, 1);
            asset.AddGlyphInfo(glyphs);

            // Get and Add Kerning Pairs to Font Asset

            KerningTable kerningTable = GetKerningTable(font.rawpath, (int)face.PointSize);
            asset.AddKerningInfo(kerningTable);

            // Add Line Breaking Rules
            //LineBreakingTable lineBreakingTable = new LineBreakingTable();
            //

            // Add Font Atlas as Sub-Asset
            asset.atlas = _atlasInfo;
            _atlasInfo.name = font.id + " Atlas";
            _atlasInfo.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(_atlasInfo, asset);

            // Create new Material and Add it as Sub-Asset
            Shader default_Shader = Shader.Find("TextMeshPro/Distance Field"); //m_shaderSelection;
            Material tmp_material = new Material(default_Shader);

            tmp_material.name = _atlasInfo + " Material";
            tmp_material.SetTexture(ShaderUtilities.ID_MainTex, _atlasInfo);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, _atlasInfo.width);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, _atlasInfo.height);

            tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, asset.normalStyle);
            tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, asset.boldStyle);

            int spread = _render >= RenderModes.DistanceField16 ? _padding + 1 : _padding;
            tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

            asset.material = tmp_material;
            tmp_material.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(tmp_material, asset);

            font.asset = asset;
            font.ImportDictionary();
            AssetDatabase.SaveAssets();
        }

        // Convert from FT_FaceInfo to FaceInfo
        static FaceInfo GetFaceInfo(FT_FaceInfo ft_face, int scaleFactor)
        {
            FaceInfo face = new FaceInfo();

            face.Name = ft_face.name;
            face.PointSize = (float)ft_face.pointSize / scaleFactor;
            face.Padding = ft_face.padding / scaleFactor;
            face.LineHeight = ft_face.lineHeight / scaleFactor;
            face.CapHeight = 0;
            face.Baseline = 0;
            face.Ascender = ft_face.ascender / scaleFactor;
            face.Descender = ft_face.descender / scaleFactor;
            face.CenterLine = ft_face.centerLine / scaleFactor;
            face.Underline = ft_face.underline / scaleFactor;
            face.UnderlineThickness = ft_face.underlineThickness == 0 ? 5 : ft_face.underlineThickness / scaleFactor; // Set Thickness to 5 if TTF value is Zero.
            face.SuperscriptOffset = face.Ascender;
            face.SubscriptOffset = face.Underline;
            face.SubSize = 0.5f;
            //face.CharacterCount = ft_face.characterCount;
            face.AtlasWidth = ft_face.atlasWidth / scaleFactor;
            face.AtlasHeight = ft_face.atlasHeight / scaleFactor;

            return face;
        }

        // Convert from FT_GlyphInfo[] to GlyphInfo[]
        static TMP_Glyph[] GetGlyphInfo(FT_GlyphInfo[] ft_glyphs, int scaleFactor)
        {
            List<TMP_Glyph> glyphs = new List<TMP_Glyph>();
            List<int> kerningSet = new List<int>();

            for (int i = 0; i < ft_glyphs.Length; i++)
            {
                TMP_Glyph g = new TMP_Glyph();

                g.id = ft_glyphs[i].id;
                g.x = ft_glyphs[i].x / scaleFactor;
                g.y = ft_glyphs[i].y / scaleFactor;
                g.width = ft_glyphs[i].width / scaleFactor;
                g.height = ft_glyphs[i].height / scaleFactor;
                g.xOffset = ft_glyphs[i].xOffset / scaleFactor;
                g.yOffset = ft_glyphs[i].yOffset / scaleFactor;
                g.xAdvance = ft_glyphs[i].xAdvance / scaleFactor;

                // Filter out characters with missing glyphs.
                if (g.x == -1)
                    continue;

                glyphs.Add(g);
                kerningSet.Add(g.id);
            }

            m_kerningSet = kerningSet.ToArray();

            return glyphs.ToArray();
        }

        static KerningTable GetKerningTable(string fontFilePath, int pointSize)
        {
            KerningTable kerningInfo = new KerningTable();
            kerningInfo.kerningPairs = new List<KerningPair>();

            // Temporary Array to hold the kerning pairs from the Native Plug-in.
            FT_KerningPair[] kerningPairs = new FT_KerningPair[7500];

            int kpCount = TMPro_FontPlugin.FT_GetKerningPairs(fontFilePath, m_kerningSet, m_kerningSet.Length, kerningPairs);

            for (int i = 0; i < kpCount; i++)
            {
                // Proceed to add each kerning pairs.
                KerningPair kp = new KerningPair(kerningPairs[i].ascII_Left, kerningPairs[i].ascII_Right, kerningPairs[i].xAdvanceOffset * pointSize);

                // Filter kerning pairs to avoid duplicates
                int index = kerningInfo.kerningPairs.FindIndex(item => item.AscII_Left == kp.AscII_Left && item.AscII_Right == kp.AscII_Right);

                if (index == -1)
                    kerningInfo.kerningPairs.Add(kp);
                else
                    if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Kerning Key for [" + kp.AscII_Left + "] and [" + kp.AscII_Right + "] is a duplicate.");
            }

            return kerningInfo;
        }

        public static void SetupGUI(TexAsset asset)
        {
            EditorGUI.BeginDisabledGroup(onRendering);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            GUILayout.Label("Atlas Resolution", GUILayout.Width(EditorGUIUtility.labelWidth));
            _bufferWidth = EditorGUILayout.IntPopup(_bufferWidth, FontResolutionLabels, FontAtlasResolutions); //, GUILayout.Width(80));
            _bufferHeight = EditorGUILayout.IntPopup(_bufferHeight, FontResolutionLabels, FontAtlasResolutions); //, GUILayout.Width(80));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Padding", GUILayout.Width(EditorGUIUtility.labelWidth));
            _padding = EditorGUILayout.IntSlider(_padding, 1, 10);
            GUILayout.EndHorizontal();

            _render = (RenderModes)EditorGUILayout.EnumPopup(_render);
            if (GUILayout.Button("Render"))
            {
                CreateSDFAsset(asset);
            }
            EditorGUI.BeginDisabledGroup(onRendering || asset.type != TexAssetType.FontSigned);
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Delete SDF Asset"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(((TexFontSigned)asset).asset));
                asset = Deconvert((TexFontSigned)asset);
            }

            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Render All Fonts"))
                {
                    onRenderingBatch = (EditorUtility.DisplayDialogComplex("Confirm Action", "Are you sure? This will take few moments.\nAnd what will you do with existing SDF Asset?",
                    "Overwrite", "Cancel", "Skip (Faster)"));
                    onRenderingBatch = onRenderingBatch == 1 ? 0 : (onRenderingBatch == 0 ? 1 : 2);
                    if (onRenderingBatch > 0)
                    {
                        DoBatchRendering();
                    }
                }
                EditorGUI.BeginDisabledGroup(onRendering);
                GUI.backgroundColor = new Color(0.5f, 0, 0.5f);
                if (GUILayout.Button("Delete All SDF Asset"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion", "Are you sure you want to delete ALL SDF Font Asset?", "Yes", "No"))
                    {
                        var fonts = TEXPreference.main.fonts;
                        for (int i = 0; i < fonts.Length; i++)
                        {
                            EditorUtility.DisplayProgressBar("Please wait", "Reimporting Fonts...", i / (float)fonts.Length);
                            if (fonts[i].type == TexAssetType.FontSigned)
                            {
                                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(((TexFontSigned)fonts[i]).asset));
                                Deconvert((TexFontSigned)fonts[i]);
                            }
                        }
                        TEXPreference.main.RebuildMaterial();
                        EditorUtility.ClearProgressBar();
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.EndDisabledGroup();
            if (onRendering)
            {
                var prog = TMPro_FontPlugin.Check_RenderProgress();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), prog, prog.ToString("P"));
                if (onRenderingBatch > 0)
                {
                    if (onRenderingBatch == 3)
                        EditorGUILayout.HelpBox("Will be stopped after current rendering is done", MessageType.Info);
                    else
                    {
                        GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button("Cancel"))
                            onRenderingBatch = 3;
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
        }
    }
}

#endif
