using System;
using System.Collections.Generic;
using TexDrawLib;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("TEXDraw/TEXDraw 3D", 2), ExecuteInEditMode]
public class TEXDraw3D : MonoBehaviour, ITEXDraw
{
    public TEXPreference preference { get { return TEXPreference.main; } }

    private const string assetID = "TEXDraw 3D Instance";

    [TextArea(3, 15), SerializeField]
    private string m_Text = "TEXDraw";

    public virtual string text
    {
        get
        {
            return m_Text;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                Redraw();
            }
            else if (m_Text != value)
            {
                m_Text = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    private int m_FontIndex = -1;

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
                Redraw();
            }
        }
    }

    [SerializeField]
    private float
        m_Size = 10f;

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
                Redraw();
            }
        }
    }

    [Range(1, 200)]
    [SerializeField]
    private int
        m_FontSize = 40;

    public virtual int fontSize
    {
        get
        {
            return m_FontSize;
        }
        set
        {
            if (m_FontSize != value)
            {
                m_FontSize = Mathf.Max(value, 1);
                Redraw();
            }
        }
    }

    [SerializeField]
    private Color
        m_Color = Color.white;

    public virtual Color color
    {
        get
        {
            return m_Color;
        }
        set
        {
            if (m_Color != value)
            {
                m_Color = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    private Material
        m_Material;

    public virtual Material material
    {
        get
        {
            return m_Material;
        }
        set
        {
            if (m_Material != value)
            {
                m_Material = value;
                Repaint();
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
                m_AutoFit = value;
                Redraw();
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
                Redraw();
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
                Redraw();
            }
        }
    }

    [SerializeField, Range(0, 2)]
    private float m_SpaceSize = 0.2f;

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
                Redraw();
            }
        }
    }

    [SerializeField]
    private Vector2
        m_Align = new Vector2(0.5f, 0.5f);

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
                Redraw();
            }
        }
    }

    [SerializeField]
    private Fitting
        m_AutoFit = Fitting.DownScale;

    [SerializeField]
    private Wrapping
        m_AutoWrap = Wrapping.NoWrap;

    [SerializeField]
    private Filling
        m_AutoFill = Filling.None;

    public string debugReport = string.Empty;

#if UNITY_EDITOR

    private void Reset()
    {
        GetComponent<MeshRenderer>().material = preference.defaultMaterial;
    }

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

    private void OnEnable()
    {
        if (!preference)
        {
            TEXPreference.Initialize();
        }

        Font.textureRebuilt += OnFontRebuild;
        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = assetID;
            mesh.hideFlags = HideFlags.DontSave;
        }
        Repaint();
        Redraw();
    }

    private DrawingContext m_drawingContext;

    public DrawingContext drawingContext
    {
        get
        {
            if (m_drawingContext == null)
                m_drawingContext = new DrawingContext(this);
            return m_drawingContext;
        }
    }

    private DrawingParams cacheParam;

    public DrawingParams drawingParams
    {
        get
        {
            if (cacheParam == null)
                (cacheParam = new DrawingParams()).hasRect = false;
            return cacheParam;
        }
    }

    private Mesh mesh;

    public void SetTextDirty()
    {
        Redraw();
    }

    public void SetTextDirty(bool now)
    {
        Redraw();
    }

    private void LateUpdate()
    {
        if (transform.hasChanged && (drawingParams.hasRect || autoFill == Filling.WorldContinous))
        {
            Redraw();
            transform.hasChanged = false;
        }
    }

    private bool _onRendering = false;

    public void Redraw()
    {
        // Multi-threading issue with OnFontRebuilt
        if (_onRendering)
            return;
        if (isActiveAndEnabled)
        {
            _onRendering = true;
            if (drawingContext == null)
                OnEnable();
            try
            {
#if UNITY_EDITOR
                if (preference.editorReloading)
                {
                    _onRendering = false;
                    return;
                }
#endif
                if (supplementIsDirty)
                    UpdateSupplements();
                drawingContext.Parse(PerformSupplements(m_Text), out debugReport);
                GenerateParam();
                drawingContext.BoxPacking(cacheParam);
                drawingContext.Render(mesh, cacheParam);
                PerformMeshEffect(mesh);
                PerformAutoLayout();
                mesh.RecalculateBounds();
                GetComponent<MeshFilter>().mesh = mesh;
                _onRendering = false;
            }
            catch (System.Exception)
            {
                _onRendering = false;
            }
        }
    }

    public void Repaint()
    {
        var m = GetComponent<MeshRenderer>().material = m_Material ? m_Material : preference.defaultMaterial;
        drawingContext.calculateNormal = m.GetTag("TexMaterialType", false, "Standard") == "Surface";
    }

    private void PerformAutoLayout()
    {
        if (!cacheParam.hasRect)
            return;
        if (cacheParam.autoFit == Fitting.RectSize)
            rectTransform.sizeDelta = cacheParam.layoutSize;
        else if (cacheParam.autoFit == Fitting.HeightOnly)
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, cacheParam.layoutSize.y);
        else
            return;
        rectTransform.hasChanged = false;
    }

    private RectTransform _cachedRectTransform;

    public RectTransform rectTransform
    {
        get
        {
            return _cachedRectTransform ? _cachedRectTransform : (_cachedRectTransform = GetComponent<RectTransform>());
        }
    }

    public void GenerateParam()
    {
        if (cacheParam == null)
        {
            cacheParam = new DrawingParams();
            cacheParam.hasRect = false;
            cacheParam.rectArea = new Rect();
        }
        cacheParam.hasRect = rectTransform;
        cacheParam.rectArea = cacheParam.hasRect ? rectTransform.rect : new Rect();
        cacheParam.pivot = cacheParam.hasRect ? rectTransform.pivot : Vector2.zero;
        cacheParam.alignment = m_Align;
        cacheParam.color = color;
        cacheParam.fontSize = m_FontSize;
        cacheParam.fontIndex = m_FontIndex;
        cacheParam.scale = m_Size;
        cacheParam.autoFit = m_AutoFit;
        cacheParam.autoWrap = cacheParam.hasRect && m_AutoFit == Fitting.RectSize ? Wrapping.NoWrap : m_AutoWrap;
        cacheParam.autoFill = m_AutoFill;
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(mesh);
        }
        else
        {
#endif
            Destroy(mesh);
#if UNITY_EDITOR
        }
#endif
    }

    private void OnDisable()
    {
        Font.textureRebuilt -= OnFontRebuild;
    }

    private void OnFontRebuild(Font f)
    {
        Redraw();
    }

    #endregion

    #region Supplements

    [NonSerialized]
    private List<TEXDrawSupplementBase> supplements = new List<TEXDrawSupplementBase>();

    private bool supplementIsDirty = false;

    [NonSerialized]
    private List<BaseMeshEffect> postEffects = new List<BaseMeshEffect>();

    public void SetSupplementDirty()
    {
        supplementIsDirty = true;
        SetTextDirty();
    }

    private void UpdateSupplements()
    {
        GetComponents(supplements);
        GetComponents(postEffects);

        supplementIsDirty = false;
    }

    private string PerformSupplements(string original)
    {
        if (supplements == null)
            return original;
        for (int i = 0; i < supplements.Count; i++)
        {
            if (supplements[i] && supplements[i].enabled)
                original = supplements[i].ReplaceString(original);
        }
        return original;
    }

    private void PerformMeshEffect(Mesh m)
    {
        if (postEffects.Count == 0)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            GetComponents(postEffects);
#endif
        for (int i = 0; i < postEffects.Count; i++)
        {
            if (postEffects[i] && postEffects[i].enabled)
                postEffects[i].ModifyMesh(m);
        }
    }

    #endregion
}
