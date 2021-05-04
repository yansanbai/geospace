using System.Collections;
using System.Linq;
using TexDrawLib;
using UnityEngine;

[RequireComponent(typeof(TEXInput))]
[TEXSupHelpTip("Add TEXInput cursor to screen")]
[AddComponentMenu("TEXDraw/TEXInput Cursor")]
public class TEXInputCursor : TEXDrawMeshEffectBase
{
    public Color activeColor = new Color32(0x00, 0x77, 0xCC, 0x99);
    public Color idleColor = new Color32(0x80, 0x80, 0x80, 0x00);
    public float cursorWidth = 2;
    public float cursorBlink = 0;

    private Coroutine blinkCoroutine = null;
    private float blinkStartTime = 0;
    private bool isBlinkTime = false;
    private bool isHot = false;
    private bool isActive = false;

    public override void ModifyMesh(Mesh mesh)
    {
        var input = GetComponent<TEXInput>();
        
        if (!input.IsInteractable())
            return;

        if (!(isActive = input.hasFocus) && idleColor.a < 1e-4f)
            return;

        if (!isHot && isBlinkTime)
            return;

        var logger = input.logger;
        var verts = new FillHelper();
        var param = tex.drawingParams;
        var scale = param.scaleFactor;
        var start = input.selectionStart;
        var length = input.selectionLength;
        var links = tex.drawingContext.linkBoxRect;
        var color = (Color32)(isActive ? activeColor : idleColor);
        var blocks = logger.GetBlockMatches(start, length).ToArray();

        verts.ReadMesh(mesh);
        
        if (links.Count == 0)
        {
            // no target to draw. guess it
            var offset = param.offset;
            DrawQuad(verts, new Rect(offset.x - cursorWidth,
                offset.y - lineDepth, cursorWidth * 2,
                lineHeight), color);
        }
        else if (blocks.Length == 0)
        {
            var i = input.selectionStart;

            var b = logger.GetBlockBefore(i);

            if (b.index == -1)
            {
                // nothing found
                var f = param.formulas[0];
                Draw(verts, ExtractAreaOfLine(scale, f), input.selectionStart > 0);
            }
            else if (b.lineSeparator >= 0 && b.start == i)
            {
                // it's different
                int s, e;
                param.GetLineRange(b.lineSeparator, out s, out e);
                var f = param.formulas[s + e - 1];
                Draw(verts, ExtractAreaOfLine(scale, f), true);
            }
            else if (b.length == 0 && b.start == i)
            {
                // placeholder
                DrawQuad(verts, links[b.index], color);
            }
            else if (i >= b.end)
            {
                // wait wait
                if (i != b.end && i == tex.text.Length)
                {
                    var f = param.formulas[param.formulas.Count - 1];
                    Draw(verts, ExtractAreaOfLine(scale, f), true);
                }
                else
                    // draw things 'after' so offset by - 1
                    Draw(verts, links[b.index], true);
            }
            else if (i >= b.start)
            {
                // wait wait
                var prev = logger.GetPrevious(b);
                if (prev.index != b.index && prev.end >= b.start)
                    Draw(verts, links[prev.index], true);
                else
                    // draw things like usual
                    Draw(verts, links[b.index], false);
            }
        }
        else
        {
            // just simple block
            foreach (var b in blocks)
            {
                if (b.lineSeparator >= 0)
                {
                    // newlines are not visible so...
                    int s, e;
                    param.GetLineRange(b.lineSeparator, out s, out e);
                    var f = param.formulas[s + e - 1];
                    Draw(verts, ExtractAreaOfLine(scale, f), true);
                    Draw(verts, links[b.index], true);
                }
                else
                    DrawQuad(verts, links[b.index], color);
            }
        }

        verts.FillMesh(mesh, tex.drawingContext.calculateNormal);
    }

    private Rect ExtractAreaOfLine(float scale, TexRenderer f)
    {
        return new Rect(f.X * scale, (f.Y * scale - lineDepth), f.Width * scale, lineHeight);
    }

    private void Draw(FillHelper verts, Rect r, bool onTheRight)
    {
        DrawQuad(verts, new Rect(r.x - cursorWidth + (onTheRight ? r.width : 0),
            r.y, cursorWidth * 2, r.height), isActive ? activeColor : idleColor);
    }

    private float lineHeight { get { return (TexUtility.spaceHeight + TexUtility.spaceDepth) * tex.drawingParams.scaleFactor; } }

    private float lineDepth { get { return (TexUtility.spaceDepth) * tex.drawingParams.scaleFactor; } }

    public static void DrawQuad(FillHelper vertex, Rect v, Color32 c)
    {
        var r = new Vector2(TexUtility.blockFontIndex, 0);
        var z = new Vector2();
        vertex.AddVert(new Vector3(v.xMin, v.yMin), c, z, r);
        vertex.AddVert(new Vector3(v.xMax, v.yMin), c, z, r);
        vertex.AddVert(new Vector3(v.xMax, v.yMax), c, z, r);
        vertex.AddVert(new Vector3(v.xMin, v.yMax), c, z, r);

        vertex.AddQuad();
    }

    public static Rect Union(Rect r1, Rect r2)
    {
        return Rect.MinMaxRect(Mathf.Min(r1.x, r2.x), Mathf.Min(r1.y, r2.y),
            Mathf.Max(r1.xMax, r2.xMax), Mathf.Max(r1.yMax, r2.yMax));
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (cursorBlink > 0)
        {
            blinkCoroutine = StartCoroutine(CaretBlink());
        }
    }

    public bool hotState
    {
        get { return isHot; }
        set
        {
            isHot = value;
            blinkStartTime = Time.unscaledTime;
            isBlinkTime = false;
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(CaretBlink());
        }
    }

    IEnumerator CaretBlink()
    {
        // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
        var input = GetComponent<TEXInput>();
        isBlinkTime = false;
        blinkStartTime = Time.unscaledTime;
        yield return null;

        while (input.hasFocus && cursorBlink > 0)
        {
            // the caret should be ON if we are in the first half of the blink period
            bool blinkState = (Time.unscaledTime - blinkStartTime) % cursorBlink > cursorBlink / 2;
            if (isBlinkTime != blinkState)
            {
                isBlinkTime = blinkState;
                if (!isHot)
                    tex.SetTextDirty(true);
            }

            // Then wait again.
            yield return null;
        }
        blinkCoroutine = null;
    }

}

