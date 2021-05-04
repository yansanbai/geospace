using System;
using System.Collections.Generic;
using TexDrawLib;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("TEXDraw/TEXDraw UI", 1)]
public class TEXDraw : MaskableGraphic, ITEXDraw, ILayoutElement, ILayoutSelfController
{
    public TEXPreference preference { get { return TEXPreference.main; } }

    private DrivenRectTransformTracker layoutTracker;

    public string debugReport = string.Empty;

    [NonSerialized]
    private bool m_TextDirty = true;

    [NonSerialized]
    private bool m_BoxDirty = false;

    [SerializeField]
    private string m_Text = "TEXDraw";

    [SerializeField]
    private int m_FontIndex = -1;

    [SerializeField]
    private float m_Size = 50f;

    [SerializeField]
    private Fitting m_AutoFit = Fitting.DownScale;

    [SerializeField]
    private Wrapping m_AutoWrap = 0;

    [SerializeField]
    private Filling m_AutoFill = 0;

    [SerializeField, Range(0, 2)]
    private float m_SpaceSize = 0.2f;

    [SerializeField]
    private Vector2 m_Align = new Vector2(0.5f, 0.5f);

    public virtual string text
    {
        get
        {
            return m_Text;
        }
        set
        {
            if (m_Text != value)
            {
                m_Text = value;
                SetTextDirty(true);
            }
        }
    }

    public virtual int fontIndex
    {
        get
        {
            return m_FontIndex;
        }
        set
        {
            if (m_FontIndex != value)
            {
                m_FontIndex = Mathf.Clamp(value, -1, 31);
                SetTextDirty(true);
            }
        }
    }

    public virtual float size
    {
        get
        {
            return m_Size;
        }
        set
        {
            if (m_Size != value)
            {
                m_Size = Mathf.Max(value, 0f);
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual Fitting autoFit
    {
        get
        {
            return m_AutoFit;
        }
        set
        {
            if (m_AutoFit != value)
            {
                layoutTracker.Clear();
                m_AutoFit = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual Wrapping autoWrap
    {
        get
        {
            return m_AutoWrap;
        }
        set
        {
            if (m_AutoWrap != value)
            {
                m_AutoWrap = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    [Obsolete("This property has been moved to TexConfiguration as project-wide configuration")]
    public virtual float spaceSize
    {
        get
        {
            return m_SpaceSize;
        }
        set
        {
            if (m_SpaceSize != value)
            {
                m_SpaceSize = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual Filling autoFill
    {
        get
        {
            return m_AutoFill;
        }
        set
        {
            if (m_AutoFill != value)
            {
                m_AutoFill = value;
                SetVerticesDirty();
            }
        }
    }

    public virtual Vector2 alignment
    {
        get
        {
            return m_Align;
        }
        set
        {
            if (m_Align != value)
            {
                m_Align = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    private static void FixCanvas(Canvas c)
    {
        if (c)
            c.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
    }

    protected override void OnEnable()
    {
        if (!preference)
        {
            TEXPreference.Initialize();
        }

        FixCanvas(canvas);
        base.OnEnable();
        m_TextDirty = true;
        UpdateSupplements();
        Font.textureRebuilt += TextureRebuilted;

   }

    protected override void OnDisable()
    {
        Font.textureRebuilt -= TextureRebuilted;
        base.OnDisable();
        layoutTracker.Clear();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        FixCanvas(canvas);
    }

    private void TextureRebuilted(Font obj)
    {
        Invoke("SetVerticesDirty", 0);
    }

#if UNITY_EDITOR

    [ContextMenu("Open Preference")]
    private void OpenPreference()
    {
        UnityEditor.Selection.activeObject = preference;
    }

    [ContextMenu("Copy Raw Syntax")]
    private void CopySupplementPreview()
    {
        GUIUtility.systemCopyBuffer = PerformSupplements(m_Text);
    }

#endif

    #region Engine

    public DrawingParams drawingParams { get; set; }
    public DrawingContext drawingContext { get; set; }

    public TEXDraw()
    {
        drawingParams = new DrawingParams() { hasRect = true };
        drawingContext = new DrawingContext(this);
    }

    protected virtual void FillMesh(Mesh m)
    {
#if UNITY_EDITOR
        if (preference.editorReloading)
            return;
#endif

        CheckTextDirty();
        drawingContext.Render(m, drawingParams);
    }

    public void SetTextDirty() { SetTextDirty(false); }

    public void SetTextDirty(bool forceRedraw)
    {
        m_BoxDirty = true;
        m_TextDirty = true;
        if (forceRedraw)
            SetAllDirty();
    }

    private void CheckTextDirty()
    {
#if UNITY_EDITOR
        if (preference.editorReloading)
            return;
#endif
        if (m_TextDirty)
        {
            drawingContext.Parse(PerformSupplements(m_Text), out debugReport, m_FontIndex);
            m_TextDirty = false;
        }
        if (m_BoxDirty || (drawingParams.rectArea != rectTransform.rect))
        {
            GenerateParam();
            drawingContext.BoxPacking(drawingParams);
            m_BoxDirty = false;
        }
    }

    public override void SetVerticesDirty()
    {
        m_BoxDirty = true;
        base.SetVerticesDirty();
    }

    public void GenerateParam()
    {
        drawingParams.autoFit = m_AutoFit;
        drawingParams.autoWrap = m_AutoFit == Fitting.RectSize ? Wrapping.NoWrap : m_AutoWrap;
        drawingParams.autoFill = m_AutoFill;
        drawingParams.alignment = m_Align;
        drawingParams.color = color;
        drawingParams.fontIndex = m_FontIndex;
        drawingParams.fontStyle = 0;
        drawingParams.fontSize = (int)(m_Size * (canvas ? canvas.scaleFactor : 1));
        drawingParams.pivot = rectTransform.pivot;
        drawingParams.rectArea = rectTransform.rect;
        drawingParams.scale = m_Size;
        drawingContext.calculateNormal = (canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.Normal) > 0;
    }

    public override Material defaultMaterial { get { return preference.defaultMaterial; } }

    // Debugging purpose
    public Mesh m_mesh;

    protected override void UpdateGeometry()
    {
        FillMesh(workerMesh);
        m_mesh = workerMesh;
        PerformPostEffects(workerMesh);
        canvasRenderer.SetMesh(workerMesh);
    }

    private void LateUpdate()
    {
        if (transform.hasChanged && autoFill == Filling.WorldContinous)
        {
            SetTextDirty(true);
            transform.hasChanged = false;
        }
    }

    #endregion

    #region Layout

    public virtual void CalculateLayoutInputHorizontal() { }

    public virtual void CalculateLayoutInputVertical() { }

    public virtual void SetLayoutHorizontal()
    {
        CheckTextDirty();
        layoutTracker.Clear();
        if (m_AutoFit == Fitting.RectSize)
        {
            layoutTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, drawingParams.layoutSize.x);
        }
    }

    public virtual void SetLayoutVertical()
    {
        CheckTextDirty();
        if (m_AutoFit == Fitting.RectSize || m_AutoFit == Fitting.HeightOnly)
        {
            layoutTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, drawingParams.layoutSize.y);
        }
    }

    public virtual float minWidth { get { return -1; } }

    public virtual float preferredWidth
    {
        get
        {
            CheckTextDirty();
            return drawingParams.layoutSize.x;
        }
    }

    public virtual float flexibleWidth { get { return -1; } }

    public virtual float minHeight { get { return -1; } }

    public virtual float preferredHeight
    {
        get
        {
            CheckTextDirty();
            return drawingParams.layoutSize.y;
        }
    }

    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 0; } }

    #endregion

    #region Supplements

    private List<BaseMeshEffect> postEffects = new List<BaseMeshEffect>();
    private List<TEXDrawSupplementBase> supplements = new List<TEXDrawSupplementBase>();

    public void SetSupplementDirty()
    {
        UpdateSupplements();
        SetTextDirty(true);
    }

    private void UpdateSupplements()
    {
        GetComponents(supplements);
        GetComponents(postEffects);
    }

    private string PerformSupplements(string original)
    {
        if (supplements == null)
            return original;
        TEXDrawSupplementBase s;
        for (int i = 0; i < supplements.Count; i++)
            if ((s = supplements[i]) && s.enabled)
                original = s.ReplaceString(original);

        return original;
    }

    private void PerformPostEffects(Mesh m)
    {
        if (postEffects == null)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            GetComponents(postEffects);
#endif
        BaseMeshEffect p;
        for (int i = 0; i < postEffects.Count; i++)
            if ((p = postEffects[i]) && p.enabled)
                p.ModifyMesh(m);
    }

    #endregion
}
