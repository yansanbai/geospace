using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StyleManager : MonoBehaviour
{
    public Style Style;

    const int FONT_SIZE = 50;
    const float CHARACTER_SIZE_RATIO = 500f;

    [HideInInspector] public static Font Font;
    [HideInInspector] public static int SignSize;
    [HideInInspector] public static int GizmoTextSize;
    [HideInInspector] public static int AxisSignSize;
    [HideInInspector] public static int PointSignSize;

    [HideInInspector] public static Color Text;
    [HideInInspector] public static Color Background;
    [HideInInspector] public static Color Card;
    [HideInInspector] public static Color Default;
    [HideInInspector] public static Color[] Themes;

    [HideInInspector] public static Color Highlight;
    [HideInInspector] public static Color Error;
    [HideInInspector] public static Color Valid;

    [HideInInspector] public static Color Geometry;
    [HideInInspector] public static Color GeometryS;
    [HideInInspector] public static Color Point;

    [HideInInspector] public static Color Line;
    [HideInInspector] public static Color Plane;
    [HideInInspector] public static Color Sign;
    [HideInInspector] public static Color Gizmo;
    [HideInInspector] public static Color Grid;
    [HideInInspector] public static Color AxisX;
    [HideInInspector] public static Color AxisY;
    [HideInInspector] public static Color AxisZ;
    [HideInInspector] public static Color ElementH;

    [HideInInspector] public static Color NavAxisX;
    [HideInInspector] public static Color NavAxisXS;
    [HideInInspector] public static Color NavAxisY;
    [HideInInspector] public static Color NavAxisYS;
    [HideInInspector] public static Color NavAxisZ;
    [HideInInspector] public static Color NavAxisZS;
    [HideInInspector] public static Color NavAxisW;
    [HideInInspector] public static Color NavAxisWS;
    [HideInInspector] public static Color NavAxisH;
    [HideInInspector] public static Color Center;
    [HideInInspector] public static Color CenterS;

    [HideInInspector] public static Material GeometryMaterial;
    [HideInInspector] public static Material GizmosLineMaterial;
    [HideInInspector] public static Material GridMaterial;
    [HideInInspector] public static Material AxisMaterial;
    [HideInInspector] public static Material ArrowMaterial;
    [HideInInspector] public static Material SignMaterial;
    [HideInInspector] public static Material SignBackgroundMaterial;
    [HideInInspector] public static Material NavAxisMaterial;
    [HideInInspector] public static Material CenterMaterial;

    public static event Action OnStyleChange;


    void Start()
    {
        if (Style)
            SetStyle(Style);
    }

    public void RefreshStyle()
    {
        if (Style != null)
            SetStyle(Style);
    }

    public void SetStyle(Style t)
    {
        Style = t;

        Font = Style.Font;
        SignSize = Style.SignSize;
        GizmoTextSize = Style.GizmoTextSize;
        AxisSignSize = Style.AxisSignSize;
        PointSignSize = Style.PointSignSize;
        Text = Style.Text;
        Background = Style.Background;
        Card = Style.Card;
        Default = Style.Default;
        Themes = Style.Themes;
        Highlight = Style.Highlight;
        Error = Style.Error;
        Valid = Style.Valid;

        Geometry = Style.Geometry;
        GeometryS = Style.GeometryS;
        Point = Style.Point;
        Line = Style.Line;
        Plane = Style.Plane;
        ElementH = Style.ElementH;
        Sign = Style.Sign;
        Gizmo = Style.Gizmo;
        Grid = Style.Grid;
        AxisX = Style.AxisX;
        AxisY = Style.AxisY;
        AxisZ = Style.AxisZ;
        NavAxisX = Style.NavAxisX;
        NavAxisXS = Style.NavAxisXS;
        NavAxisY = Style.NavAxisY;
        NavAxisYS = Style.NavAxisYS;
        NavAxisZ = Style.NavAxisZ;
        NavAxisZS = Style.NavAxisZS;
        NavAxisW = Style.NavAxisW;
        NavAxisWS = Style.NavAxisWS;
        NavAxisH = Style.NavAxisH;
        Center = Style.Center;
        CenterS = Style.CenterS;
        GeometryMaterial = Style.GeometryMaterial;
        GizmosLineMaterial = Style.GizmosLineMaterial;
        GridMaterial = Style.GridMaterial;
        AxisMaterial = Style.AxisMaterial;
        ArrowMaterial = Style.ArrowMaterial;
        SignMaterial = Style.SignMaterial;
        SignBackgroundMaterial = Style.SignBackgroundMaterial;
        NavAxisMaterial = Style.NavAxisMaterial;
        CenterMaterial = Style.CenterMaterial;

        if (OnStyleChange != null)
            OnStyleChange();
    }

    public static void SetGeometryMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = GeometryMaterial;
    }

    public static void SetGizmosLineMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = GizmosLineMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Gizmo);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetGizmoTextAttr(TextMesh textMesh)
    {
        textMesh.font = Font;
        textMesh.fontSize = FONT_SIZE;
        textMesh.characterSize = GizmoTextSize / CHARACTER_SIZE_RATIO;
    }

    public static void SetGizmoTextMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Gizmo);
        renderer.SetPropertyBlock(prop);
    }


    public static void SetPointProperty(MeshRenderer renderer, int colorIndex = 0)
    {
        colorIndex = ClampColorIndex(colorIndex);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", colorIndex == 0 ? Point : Themes[colorIndex - 1]);
        prop.SetColor("_ColorH", ElementH);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetLineProperty(MeshRenderer renderer, int colorIndex = 0)
    {
        colorIndex = ClampColorIndex(colorIndex);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", colorIndex == 0 ? Line : Themes[colorIndex - 1]);
        prop.SetColor("_ColorH", ElementH);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetLineProperty(LineRenderer renderer, int colorIndex = 0)
    {
        colorIndex = ClampColorIndex(colorIndex);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", colorIndex == 0 ? Line : Themes[colorIndex - 1]);
        prop.SetColor("_ColorH", ElementH);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetPlaneProperty(MeshRenderer renderer, int colorIndex = 0)
    {
        colorIndex = ClampColorIndex(colorIndex);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", colorIndex == 0 ? Plane : Themes[colorIndex - 1]);
        prop.SetColor("_ColorH", ElementH);
        renderer.SetPropertyBlock(prop);
    }

    private static int ClampColorIndex(int colorIndex)
    {
        if (colorIndex < 0)
            colorIndex = 0;
        else if (colorIndex > Themes.Length)
            colorIndex = Themes.Length;

        return colorIndex;
    }


    public static void SetSignAttr(TextMesh textMesh)
    {
        textMesh.font = Font;
        textMesh.fontSize = FONT_SIZE;
        textMesh.characterSize = SignSize / CHARACTER_SIZE_RATIO;
    }
    public static void SetSignMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignMaterial;
        // renderer.SetPropertyBlock(SignProp);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Sign);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetSignNMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignBackgroundMaterial;
        // renderer.SetPropertyBlock(SignNProp);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Highlight);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetSignEMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignBackgroundMaterial;
        // renderer.SetPropertyBlock(SignEProp);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Error);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetSignVMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignBackgroundMaterial;
        // renderer.SetPropertyBlock(SignVProp);
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Valid);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetGridMaterial(MeshRenderer renderer, int size)
    {
        renderer.sharedMaterial = GridMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", Grid);
        prop.SetFloat("_Size", size);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisXMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = AxisMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisX);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisYMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = AxisMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisY);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisZMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = AxisMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisZ);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetArrowXMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = ArrowMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisX);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetArrowYMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = ArrowMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisY);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetArrowZMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = ArrowMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisZ);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisSignAttr(TextMesh textMesh)
    {
        textMesh.font = Font;
        textMesh.fontSize = FONT_SIZE;
        textMesh.characterSize = AxisSignSize / CHARACTER_SIZE_RATIO;
    }

    public static void SetPointSignAttr(TextMesh textMesh)
    {
        textMesh.font = Font;
        textMesh.fontSize = FONT_SIZE;
        textMesh.characterSize = PointSignSize / CHARACTER_SIZE_RATIO;
    }

    public static void SetAxisSignXMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisX);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisSignYMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisY);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetAxisSignZMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = SignMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);
        prop.SetColor("_Color", AxisZ);
        renderer.SetPropertyBlock(prop);
    }

    public static void SetNavAxisMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = NavAxisMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);

        prop.SetColor("_ColorX", NavAxisX);
        prop.SetColor("_ColorShadowX", NavAxisXS);
        prop.SetColor("_ColorY", NavAxisY);
        prop.SetColor("_ColorShadowY", NavAxisYS);
        prop.SetColor("_ColorZ", NavAxisZ);
        prop.SetColor("_ColorShadowZ", NavAxisZS);
        prop.SetColor("_ColorW", NavAxisW);
        prop.SetColor("_ColorShadowW", NavAxisWS);
        prop.SetColor("_HighlightColor", NavAxisH);
        prop.SetFloat("_Highlight", 0);

        renderer.SetPropertyBlock(prop);
    }

    public static void SetNavAxisHMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = NavAxisMaterial;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);

        prop.SetColor("_ColorX", NavAxisX);
        prop.SetColor("_ColorShadowX", NavAxisXS);
        prop.SetColor("_ColorY", NavAxisY);
        prop.SetColor("_ColorShadowY", NavAxisYS);
        prop.SetColor("_ColorZ", NavAxisZ);
        prop.SetColor("_ColorShadowZ", NavAxisZS);
        prop.SetColor("_ColorW", NavAxisW);
        prop.SetColor("_ColorShadowW", NavAxisWS);
        prop.SetColor("_HighlightColor", NavAxisH);
        prop.SetFloat("_Highlight", 1);

        renderer.SetPropertyBlock(prop);
    }

    public static void SetCenterMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = CenterMaterial;
        
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(prop);

        prop.SetColor("_Color", Center);
        prop.SetColor("_ShadowColor", CenterS);

        renderer.SetPropertyBlock(prop);
    }

    #region Singleton
    private static StyleManager _instance = null;
    public static StyleManager instance
    {
        get
        {
            if (!_instance)
            {
                // check if available
                _instance = FindObjectOfType(typeof(StyleManager)) as StyleManager;

                // create a new one
                if (!_instance)
                {
                    var obj = new GameObject("StyleManager");
                    _instance = obj.AddComponent<StyleManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    #endregion

}
