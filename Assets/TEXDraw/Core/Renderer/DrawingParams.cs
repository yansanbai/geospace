#if TEXDRAW_DEBUG && !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)

using UnityEngine.Profiling;

#endif

using UnityEngine;
using System.Collections.Generic;

namespace TexDrawLib
{
    /// <summary>
    /// A class for handling the math behind the final rendering process.
    /// All layout stuff is handled here.
    /// </summary>
    /// <exception cref="HeadacheException">
    /// We're not responsible for the headache
    /// after reading this code. Proceed with caution.
    /// </exception>
    public class DrawingParams
    {
        // ------------ Parameter must be filled by Component ------------------
        /// is it has defined rect bound?
        public bool hasRect;

        /// Auto Fit Mode: 0 = Off, 1 = Down Scale, 2 = Rect Size, 3 = Height Only, 4 = Scale, 5 = Best Fit
        public Fitting autoFit;

        /// Wrap Mode: 0 = No Wrap, 1 = Wrap Letter, 2 = Word Wrap, 3 = Word Wrap Justified
        public Wrapping autoWrap;

        /// Rectangle Area, if rect is defined
        public Rect rectArea;

        /// Scale of rendered Graphic
        public float scale;

        /// Alignment vector (respect to Unity's coordinate system)
        public Vector2 alignment;

        /// Rectangle pivot position, if rect is defined
        public Vector2 pivot;

        /// UV3 Filling: 0 = Off, 1 = Rectangle, 2 = Whole Text, 3 = WT Squared, 4 = Per line, 5 = Per word, 6 = Per character, 7 = PC Squared
        public Filling autoFill;

        // ------------ Parameter must be filled by External Component ------------------
        public Color color;

        public int fontIndex;
        public FontStyle fontStyle;
        public int fontSize;

        //Renderer Parameter
        private List<TexRenderer> m_formulas = ListPool<TexRenderer>.Get();

        public List<TexRenderer> formulas
        {
            get
            {
                return m_formulas;
            }
            set
            {
                m_formulas = value;
                PredictSize();
            }
        }

        public DrawingContext context;

        // Internal Computation Results
        public Vector2 size;

        public Vector2 offset;
        public Vector2 layoutSize;

        /// relative final scale ratio
        public float factor = 1;

        public float scaleFactor { get { return scale * factor; } }

        public DrawingParams() { }

        public void Render()
        {
            if (hasRect && (rectArea.size.x <= 0 || rectArea.size.y <= 0))
                return;

            // At first, calculate starting offset
            CalculateRenderedArea(size, alignment, rectArea);

            // We'll start from top (line-per-line). This means offset will subtracted decrementally..
            float scaleFactor = scale * factor, paragraph = 0, x, y;
            offset.y += size.y;
            FillHelper verts = context.vertex;
            for (int i = 0; i < formulas.Count; i++)
            {
                var box = formulas[i];
                int lastVerts = verts.vertexcount;
                float alignX = alignment.x, sizeX = size.x;

                if (box.usingMetaRules)
                {
                    var r = rectArea;
                    var meta = box.metaRules;
                    sizeX = box.Width * scaleFactor;
                    alignX = meta.GetAlignment(alignment.x);
                    r.width -= box.PenaltyWidth * scaleFactor;
                    // We only need to adjust horizontal offset
                    offset.x = CalculateXMetric(sizeX, alignX, r.width
                        , r.x + r.width / 2, meta.left, meta.right, box.partOfPreviousLine == 0 ? meta.leading : 0);
                }

                if (box.Box.totalHeight == 0) continue; // empty meta block

                // Get ready to render

                if (box.partOfPreviousLine == 0)
                    offset.y -= paragraph * scaleFactor;
                x = offset.x + alignX * (sizeX - scaleFactor * box.Width);
                y = offset.y -= box.Height * scaleFactor;
                box.Render(context, x, y, scaleFactor);
                offset.y -= (box.Depth + box.PenaltySpacing) * scaleFactor;
                if (box.partOfPreviousLine == 0)
                    paragraph = box.PenaltyParagraph;

                if (autoFill == Filling.PerLine)
                    RenderUV3Line(verts, new Rect(new Vector2(x, offset.y),
                        new Vector2(box.Width, box.Depth + box.Height) * scaleFactor), lastVerts);
            }
            if (autoFill > 0)
                RenderUV3(verts);
#if TEXDRAW_TMP
            FixTMP(verts);
#endif
        }

        public static Vector2 InverseLerp(Rect area, Vector2 pos)
        {
            pos.x = InverseLerp(area.xMin, area.xMax, pos.x);
            pos.y = InverseLerp(area.yMin, area.yMax, pos.y);
            return pos;
        }

        public static float InverseLerp(float a, float b, float value)
        {
            return a != b ? (value - a) / (b - a) : 0;
        }

        private float originalScale;

        public void PredictSize()
        {
            // Make a backup
            originalScale = scale;
            PredictSizeInternal();
        }

        private void PredictSizeInternal()
        {
#if TEXDRAW_DEBUG
            Profiler.BeginSample("Compositing");
#endif
            size = Vector2.zero;
            //Predict dirty draw size
            for (int i = 0; i < formulas.Count;)
            {
                var form = formulas[i];
                size.x = Mathf.Max(size.x, (form.Width + form.PenaltyWidth) * scale);
                if (form.Box.totalHeight == 0)
                    i++; // Probably an empty block but there's meta over there. Therefore, skip it
                else if (++i < formulas.Count)
                    size.y += (form.CompleteHeight) * scale;
                else
                    size.y += (form.Height + form.Depth) * scale; // The last spacing shouldn't included
            }

            layoutSize = size;
            factor = 1;

            //Zero means auto, let's change our rect size
            if (rectArea.width == 0)
                rectArea.width = layoutSize.x;
            if (rectArea.height == 0)
                rectArea.height = layoutSize.y;

            //Autowrap? only do if needed
#if TEXDRAW_DEBUG
            Profiler.BeginSample("Wrapping");
#endif

            if (autoWrap > 0 && size.x > rectArea.width)
                HandleWrapping();

#if TEXDRAW_DEBUG
            Profiler.EndSample();
#endif

            //Autofit? then resize the prediction
            if (autoFit == Fitting.Scale || autoFit == Fitting.BestFit || autoFit == Fitting.DownScale)
            {
                factor = autoFit == Fitting.Scale ? 1000 : 1;
                if (size.x > 0)
                    factor = Mathf.Min(factor, rectArea.width / size.x);
                if (size.y > 0)
                    factor = Mathf.Min(factor, rectArea.height / size.y);
                size *= factor;
            }
            if (autoFit == Fitting.BestFit)
            {
                if (factor < 1 && scale > 0.001f)
                {
                    factor = 1;
                    scale -= 1f;
                    RevertBackList();
                    //Start again (That's why it's expensive)
                    PredictSizeInternal();
                }
                else
                {
                    // Expected scale reached. now apply it
                    factor = scale / originalScale;
                    scale = originalScale;
                }
            }
#if TEXDRAW_DEBUG
            Profiler.EndSample();
#endif
        }

        public void HandleWrapping()
        {
            var _wrap = (int)autoWrap - 1;
            _useWordWrap = (_wrap % 3) >= 1;
            _doJustify = (_wrap % 3) == 2;
            var _defaultReversed = _wrap >= 3;
            _realBlankSpaceWidth = TexUtility.spaceWidth;
            size.x = 0;
            int i = 0;

            while (i < formulas.Count)
            {
                var box = formulas[i].Box as HorizontalBox;
                if (_doJustify)
                    box.Recalculate();
                // Dirty way to check if this line needs to be splitted or not
                if (box == null || (box.width + formulas[i].PenaltyWidth) * scale < rectArea.width)
                {
                    i++;
                    continue;
                }

                if (formulas[i].usingMetaRules)
                {
                    if (formulas[i].metaRules.GetWrappingReversed(_defaultReversed))
                        HandlePerLineWrappingReversed(i);
                    else
                        HandlePerLineWrapping(i);
                }
                else if (_defaultReversed)
                    HandlePerLineWrappingReversed(i);
                else
                    HandlePerLineWrapping(i);
                _spaceIdxs.Clear();

                i++;
            }

            //Rescale again
            if (rectArea.width == layoutSize.x)
                rectArea.width = size.x;
            if (rectArea.height == layoutSize.y)
                rectArea.height = size.y;
            layoutSize = size;
        }

        private float _realBlankSpaceWidth;
        private bool _doJustify;
        private bool _useWordWrap;
        private static List<int> _spaceIdxs = new List<int>();

        private void HandlePerLineWrapping(int i)
        {
            TexRenderer row = m_formulas[i], newrow;
            HorizontalBox box = (HorizontalBox)row.Box;
            List<Box> ch = box.children;

            float x = row.PenaltyWidth, xPenalty = x, lastSpaceX = 0;
            int lastSpaceIdx = -1;
            //Begin Per-character pooling
            for (int j = 0; j < ch.Count; j++)
            {
                var child = ch[j];
                //White line? make a mark
                if (child is StrutBox && ((StrutBox)child).policy == StrutPolicy.BlankSpace)
                {
                    lastSpaceIdx = j; //last space, index
                    lastSpaceX = x; //last space, x position
                    _spaceIdxs.Add(lastSpaceIdx); //record that space
                    box.width -= child.width - _realBlankSpaceWidth; // normalize (as below)
                    child.width = _realBlankSpaceWidth; // All spaces must have this width (they may modified after WordWarpJusified).
                }
                x += child.width;
                //Total length not yet to break our rect limit? continue
                if (x * scale <= rectArea.width)
                {
                    continue;
                }
                //Now j is maximum limit character length. Now move any
                //character before that to the new previous line

                //Did we use word wrap? Track the last space index
                if (_useWordWrap && lastSpaceIdx >= 0)
                {
                    j = lastSpaceIdx;
                    x = lastSpaceX;
                    //Justify too? then expand our spaces width
                    if (_doJustify && _spaceIdxs.Count > 1)
                    {
                        float normalizedWidth = rectArea.width / scale;
                        float extraWidth = (normalizedWidth - x) / (_spaceIdxs.Count - 1);
                        for (int k = 0; k < _spaceIdxs.Count; k++)
                            ch[_spaceIdxs[k]].width += extraWidth;
                        x = normalizedWidth;
                    }
                }
                else
                {
                    x -= ch[j].width;
                }
                if (j == 0 && (!_useWordWrap || lastSpaceIdx != j))
                {
                    // infinite loop prevention
                    x += ch[j].width;
                    continue;
                }
                var doOmitSpace = (_useWordWrap && lastSpaceIdx >= 0);
                if (doOmitSpace) { j++; box.width -= ch[lastSpaceIdx].width; }
                m_formulas.Insert(i, newrow = TexRenderer.Get(HorizontalBox.Get(ch.GetRangePool(0, j)),
                    row.metaRules, row.partOfPreviousLine)); //Add to previous line,
                ch.RemoveRange(0, j);
                //Update our measurements, remember now m_formulas[i] is different with box
                row.partOfPreviousLine = doOmitSpace ? 2 : 1;
                box.width -= (newrow.Box.width = x - xPenalty);
                size.x = Mathf.Max(size.x, x);
                size.y += newrow.CompleteHeight * scale;
                break;
            }
        }

        // Branched wrapping algorithm for RTL support based above.
        private void HandlePerLineWrappingReversed(int i)
        {
            TexRenderer row = m_formulas[i], newrow;
            HorizontalBox box = (HorizontalBox)row.Box;
            List<Box> ch = box.children;

            float x = row.PenaltyWidth, xPenalty = x, lastSpaceX = 0;
            int lastSpaceIdx = -1, last = ch.Count - 1;
            //Begin Per-character pooling
            for (int j = ch.Count; j-- > 0;)
            {
                var child = ch[j];
                //White line? make a mark
                if (child is StrutBox && ((StrutBox)child).policy == StrutPolicy.BlankSpace)
                {
                    lastSpaceIdx = j; //last space, index
                    lastSpaceX = x; //last space, x position
                    _spaceIdxs.Add(lastSpaceIdx); //record that space
                    child.width = _realBlankSpaceWidth; // All spaces must have this width (they may modified after WordWarpJusified).
                }
                x += ch[j].width;
                //Total length not yet to break our rect limit? continue
                if (x * scale <= rectArea.width)
                {
                    if (_doJustify && (0 == j))
                        box.Recalculate();
                    continue;
                }
                //Now j is maximum limit character length. Now move any
                //character before that to the new previous line

                //Did we use word wrap? Track the last space index
                if (_useWordWrap && lastSpaceIdx >= 0)
                {
                    j = lastSpaceIdx;
                    x = lastSpaceX;
                    //Justify too? then expand our spaces width
                    if (_doJustify && _spaceIdxs.Count > 1)
                    {
                        float normalizedWidth = rectArea.width / scale;
                        float extraWidth = (normalizedWidth - x) / (_spaceIdxs.Count - 1);
                        for (int k = 0; k < _spaceIdxs.Count; k++)
                            ch[_spaceIdxs[k]].width += extraWidth;
                        x = normalizedWidth;
                    }
                }
                else
                {
                    x -= ch[j].width;
                }
                if (j == last && (!_useWordWrap || lastSpaceIdx != j))
                {
                    // infinite loop prevention
                    x += ch[j].width;
                    continue;
                }
                var doOmitSpace = (_useWordWrap && lastSpaceIdx >= 0);
                m_formulas.Insert(i, newrow = TexRenderer.Get(HorizontalBox.Get(ch.GetRangePool(j + 1, last - j)),
                    row.metaRules, row.partOfPreviousLine)); //Add to previous line,
                if (doOmitSpace) { j--; box.width -= ch[lastSpaceIdx].width; ch[lastSpaceIdx].Flush(); }
                ch.RemoveRange(j + 1, last - j);
                //Update our measurements, remember now m_formulas[i] is different with box
                row.partOfPreviousLine = doOmitSpace ? 2 : 1;
                box.width -= newrow.Box.width = x - xPenalty;
                size.x = Mathf.Max(size.x, x);
                size.y += newrow.CompleteHeight * scale;
                break;
            }
        }

        public void RevertBackList()
        {
            var RTL = autoWrap > Wrapping.WordWrapJustified;
            var rtl = RTL;
            for (int i = 0; i < m_formulas.Count; i++)
            {
                if (m_formulas[i].partOfPreviousLine > 0)
                {
                    var box = m_formulas[i].Box as HorizontalBox;
                    var prevBox = m_formulas[i - 1].Box as HorizontalBox;

                    if (prevBox != null)
                    {
                        rtl = m_formulas[i].usingMetaRules ? m_formulas[i].metaRules.GetWrappingReversed(RTL) : rtl;
                        if (rtl)
                        {
                            prevBox.Add(0, StrutBox.GetBlankSpace());
                            prevBox.AddRange(box, 0);
                        }
                        else
                            // Space char is there, so we won't need to add anymore
                            prevBox.AddRange(box);
                    }
                    m_formulas[i].Box = null;
                    m_formulas[i].Flush();
                    m_formulas.RemoveAt(i);
                    i--;
                }
            }
        }

        public void CalculateRenderedArea(Vector2 size, Vector2 align, Rect rectArea)
        {
            if (hasRect)
            {
                //Configure offset & alignment, Just comment out one of these things if you don't understood this ;)
                offset = -(
                    VecScale(size, align) + //Make sure the drawing pivot affected with aligment
                    VecScale(rectArea.size, VecNormal(align)) + //Make sure it stick on rect bound
                    -rectArea.center); //Make sure we calculate it from center (inside) of Rect no matter transform pivot has
            }
            else
            {
                //Miss lot of features
                offset = -VecScale(size, align);
            }
        }

        public float CalculateXMetric(float width, float align, float area, float center, float left, float right, float lead)
        {
            // Just like CalculateRenderedArea, but X only
            float x = (hasRect ? area * (align - 0.5f) + center : 0) - width * align;
            return x + (left + (1 - align) * lead) * scale * factor;
        }

        private Vector2 VecScale(Vector2 a, Vector2 b)
        { return new Vector2(a.x * b.x, a.y * b.y); }

        private Vector2 VecNormal(Vector2 a)
        { return new Vector2(-a.x + 0.5f, -a.y + 0.5f); }

        private void FixTMP(FillHelper verts)
        {
            var scale = context.monoContainer.transform.lossyScale.y * factor;
            for (int i = 0; i < verts.vertexcount; i++)
            {
                verts.m_Uv1S[i] = new Vector2(verts.m_Uv1S[i].x, scale);
            }
        }

        private void RenderUV3Line(FillHelper verts, Rect uv, int idx)
        {
            // After render... uhh it's done... except if there's autoFill per line in there... which need to be done right now ...
            switch (autoFill)
            {
                case Filling.PerLine:
                    for (int j = idx; j < verts.vertexcount; j++)
                    {
                        verts.SetUV2(InverseLerp(uv, verts.m_Positions[j]), j);
                    }
                    break;
                    /*case 5:
                        //Toughest filling method: Per word - Not yet available
                        var boxes = box.Box.Children;
                        r = new Rect(offset, box.RenderSize);
                        for (int j = lastVerts; j < verts.currentVertCount; j++) {
                            verts.m_Uv2S[j] = InverseLerp(r, verts.m_Positions[j]);
                        }
                        break;*/
            }
        }

        private void RenderUV3(FillHelper verts)
        {
            var count = verts.vertexcount;
            switch (autoFill)
            {
                case Filling.Rectangle:
                    Rect r;
                    if (hasRect)
                        r = rectArea;
                    else
                        r = new Rect(-VecScale(size, alignment), size);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(InverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.WholeText:
                    if (hasRect)
                        r = new Rect(-(
                        VecScale(size, (alignment)) +
                        VecScale(rectArea.size, VecNormal(alignment)) +
                        -(rectArea.center)), size);
                    else
                        r = new Rect(-VecScale(size, alignment), size);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(InverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.WholeTextSquared:
                    if (hasRect)
                        r = new Rect(-(
                        VecScale(size, (alignment)) +
                        VecScale(rectArea.size, VecNormal(alignment)) +
                        -(rectArea.center)), size);
                    else
                        r = new Rect(-VecScale(size, alignment), size);

                    var max = Mathf.Max(r.width, r.height);
                    var center = r.center;
                    r.size = Vector2.one * max;
                    r.center = center;
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(InverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.PerCharacter:
                    for (int i = 0; i < count; i++)
                    {
                        int l = i % 4;
                        verts.SetUV2(new Vector2(l == 0 | l == 3 ? 0 : 1, l < 2 ? 0 : 1), i);
                    }
                    break;
                case Filling.PerCharacterSquared:
                    for (int i = 0; i < count; i += 4)
                    {
                        Vector2 sz = verts.m_Positions[i + 2] - verts.m_Positions[i];
                        if (sz.x <= 0 || sz.y <= 0)
                        {
                            for (int l = 0; l < 4; l++)
                            {
                                verts.SetUV2(new Vector2(l == 0 | l == 3 ? 0 : 1, l < 2 ? 0 : 1), i);
                            }
                            continue;
                        }
                        float xMin, xMax, yMin, yMax;
                        if (sz.x > sz.y)
                        {
                            var h = sz.y / sz.x;
                            xMin = 0;
                            xMax = 1;
                            yMin = (1 - h) / 2;
                            yMax = 1 - yMin;
                        }
                        else
                        {
                            var v = sz.x / sz.y;
                            yMin = 0;
                            yMax = 1;
                            xMin = (1 - v) / 2;
                            xMax = 1 - xMin;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            verts.SetUV2(new Vector2(l == 0 | l == 3 ? xMin : xMax, l < 2 ? yMin : yMax), i + l);
                        }
                    }
                    break;
                case Filling.LocalContinous:
                    var ratio = 1 / (factor * scale);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(verts.m_Positions[i] * ratio, i);
                    }
                    break;
                case Filling.WorldContinous:
                    ratio = 1 / (factor * scale);
                    var transform = context.monoContainer.transform;
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(transform.TransformPoint(verts.m_Positions[i]) * ratio, i);
                    }
                    break;
            }
        }

        public Rect GetInnerArea()
        {
            return new Rect(offset, size);
        }

        public Rect GetInnerAreaOfLine(int parsedlineindex)
        {
            var r = formulas[parsedlineindex];
            var s = scaleFactor;
            return new Rect(r.X * s, (r.Y - r.Depth) * s, r.Width * s, (r.Height + r.Depth) * s);
        }


        public Rect GetInnerAreaOfLine(int parsedlinestart, int parsedlinelength)
        {
            Rect r = GetInnerAreaOfLine(parsedlinestart);
            for (int i = 1; i < parsedlinelength; i++)
            {
                r = Union(r, GetInnerAreaOfLine(i + parsedlinestart));
            }
            return r;
        }

        public void GetLineRange(int line, out int parsedstart, out int parsedlength)
        {
            int iter = -1;
            parsedstart = -1;
            parsedlength = 0;
            for (int i = 0; i < m_formulas.Count; i++)
            {
                if (m_formulas[i].partOfPreviousLine == 0)
                    iter++;
                else continue;

                if (iter == line)
                {
                    parsedstart = i;
                }
                else if (parsedstart >= 0)
                {
                    parsedlength = i - parsedstart;
                    return;
                }
            }
            // looks like reaching the end
            parsedlength = m_formulas.Count - parsedstart;
        }

        private static Rect Union(Rect r1, Rect r2)
        {
            return Rect.MinMaxRect(Mathf.Min(r1.x, r2.x), Mathf.Min(r1.y, r2.y),
                Mathf.Max(r1.xMax, r2.xMax), Mathf.Max(r1.yMax, r2.yMax));
        }
    }
}
