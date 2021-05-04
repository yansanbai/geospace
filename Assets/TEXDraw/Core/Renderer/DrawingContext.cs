#if TEXDRAW_DEBUG && !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)

using UnityEngine.Profiling;

#endif

using UnityEngine;
using System;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class DrawingContext
    {
        public TexFormulaParser parser;
        public List<TexFormula> parsed = ListPool<TexFormula>.Get();
        public List<string> linkBoxKey = ListPool<string>.Get();
        public List<Rect> linkBoxRect = ListPool<Rect>.Get();
        public List<Color> linkBoxTint = ListPool<Color>.Get();
        private bool hasInit = false;
        public bool parsingComplete = false;
        public bool calculateNormal = false;
        public FillHelper vertex;
        public Component monoContainer;

        public DrawingContext(Component parent)
        {
            vertex = new FillHelper();
            parser = new TexFormulaParser();
            monoContainer = parent;
            hasInit = true;
        }

        public void Clear()
        {
            vertex.Clear();
        }

        // Using this variant is much faster than anything else...
        public void Draw(int id, Vector3 vPos, Vector2 vSize, Vector2 uvTL, Vector2 uvTR, Vector2 uvBR, Vector2 uvBL)
        {
            var c = new Vector2(id, 0);
            var r = TexContext.Color.value;

            vertex.AddVert(vPos, r, uvTL, c);  //Top-Left
            vPos.x += vSize.x;
            vertex.AddVert(vPos, r, uvTR, c);  //Top-Right
            vPos.y += vSize.y;
            vertex.AddVert(vPos, r, uvBR, c); //Bottom-Right
            vPos.x -= vSize.x;
            vertex.AddVert(vPos, r, uvBL, c); //Bottom-Left

            vertex.AddQuad();
        }

        public void DrawWireDebug(Rect v, Color32 c)
        {
            var r = new Vector2(TexUtility.blockFontIndex + 1, 0);
            var z = new Vector2();
            vertex.AddVert(new Vector3(v.xMin, v.yMin), c, z, r);
            vertex.AddVert(new Vector3(v.xMax, v.yMin), c, z, r);
            vertex.AddVert(new Vector3(v.xMax, v.yMax), c, z, r);
            vertex.AddVert(new Vector3(v.xMin, v.yMax), c, z, r);

            vertex.AddQuad();
        }

        public Color DrawLink(Rect v, string key)
        {
            linkBoxKey.Add(key);
            linkBoxRect.Add(v);
            if (linkBoxKey.Count > linkBoxTint.Count)
                linkBoxTint.Add(Color.white);
            return linkBoxTint[linkBoxKey.Count - 1];
        }

        public void Draw(int id, Vector2[] v, Vector2[] uv)
        {
            var c = new Vector2(id, 0);
            var r = TexContext.Color.value;

            vertex.AddVert(v[0], r, uv[0], c); //Top-Left
            vertex.AddVert(v[1], r, uv[1], c); //Top-Right
            vertex.AddVert(v[2], r, uv[2], c);  //Bottom-Right
            vertex.AddVert(v[3], r, uv[3], c); //Bottom-Left
            vertex.AddQuad();
        }

        private static readonly char[] newLineChar = new char[] { '\n' };

        public bool Parse(string input, out string errResult, int renderFont = -1)
        {
#if TEXDRAW_DEBUG
            Profiler.BeginSample("Parsing");
#endif

            if (!hasInit)
            {
                vertex = new FillHelper();
                parser = new TexFormulaParser();
            }
            try
            {
                TexContext.Font.Reset(renderFont);
                TexContext.FontMetaPushed = false;
                parsingComplete = false;
                string[] strings = input.Split(newLineChar, StringSplitOptions.None);
                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                        parsed[i].Flush();
                }
                parsed.Clear();
                for (int i = 0; i < strings.Length; i++)
                    parsed.Add(parser.Parse(strings[i]));
                parsingComplete = true;
            }
            catch (Exception ex)
            {
                errResult = ex.Message;
#if TEXDRAW_DEBUG
                Profiler.EndSample();
#endif
                // throw ex;
                return false;
            }
            errResult = string.Empty;
#if TEXDRAW_DEBUG
            Profiler.EndSample();
#endif
            return true;
        }

        public bool Parse(string input)
        {
            if (!hasInit)
            {
                vertex = new FillHelper();
                parser = new TexFormulaParser();
            }
            try
            {
                parsingComplete = false;
                string[] strings = input.Split(newLineChar, StringSplitOptions.RemoveEmptyEntries);
                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                        parsed[i].Flush();
                }
                parsed.Clear();
                for (int i = 0; i < strings.Length; i++)
                    parsed.Add(parser.Parse(strings[i]));
                parsingComplete = true;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Render(Mesh m, DrawingParams param)
        {
#if TEXDRAW_DEBUG
            Profiler.BeginSample("Rendering");
#endif

            m.Clear();
            Clear();

            if (parsingComplete)
            {
                // Color processing is happening only in rendering section.
                TexContext.Color.Reset(param.color);

                param.context = this;
                linkBoxKey.Clear();
                linkBoxRect.Clear();
                param.Render();
            }
            Push2Mesh(m);

#if TEXDRAW_DEBUG
            Profiler.EndSample();
#endif
        }

        public void BoxPacking(DrawingParams param)
        {
#if TEXDRAW_DEBUG
            Profiler.BeginSample("Boxing");
            param.formulas = ToRenderers(this.parsed, param);
            Profiler.EndSample();
#else
            param.formulas = ToRenderers(this.parsed, param);
#endif
        }

        /// Convert Atom into Boxes
        public static List<TexRenderer> ToRenderers(List<TexFormula> formulas, DrawingParams param)
        {
            // Init default parameters
            var list = param.formulas;
            TexContext.Resolution = param.fontSize;
            TexContext.Style.Reset(param.fontStyle);
            TexContext.Font.Reset(param.fontIndex);
            TexContext.Kerning.Reset(0);
            TexContext.Environment.Reset(TexEnvironment.Display);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Flush();
            }
            list.Clear();

            TexMetaRenderer lastMeta = null;
            for (int i = 0; i < formulas.Count; i++)
            {
                if (lastMeta != null || (formulas[i].AttachedMetaRenderer != null && formulas[i].AttachedMetaRenderer.enabled))
                {
                    var meta = formulas[i].AttachedMetaRenderer ?? (formulas[i].AttachedMetaRenderer = lastMeta);
                    meta.ApplyBeforeBoxing(param);
                    lastMeta = meta;
                }
                list.Add(formulas[i].GetRenderer());
            }
            return list;
        }

        protected void Push2Mesh(Mesh m)
        {
            vertex.FillMesh(m, calculateNormal);
        }
    }
}
