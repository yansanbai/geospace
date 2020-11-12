using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Style : ScriptableObject
{
    public Font Font;
    public int SignSize = 25;
    public int GizmoTextSize = 20;
    public int AxisSignSize = 20;
    public int PointSignSize = 16;

    public Color Text = Util.HexColor("#404040");
    public Color Background = Util.HexColor("#EEEEEE");
    public Color Card = Util.HexColor("#FFFFFF");

    public Color Default = Util.HexColor("#888888");
    public Color[] Themes = new Color[] {
        Util.HexColor("#F45964"),
        Util.HexColor("#FFD676"),
        Util.HexColor("#6ACB7F"),
        Util.HexColor("#44C0FA"),
        Util.HexColor("#9672D0"),
    };

    public Color Highlight = Util.HexColor("#FFD676");
    public Color Error = Util.HexColor("#F45964");
    public Color Valid = Util.HexColor("#6ACB7F");

    //
    public Color Geometry = Util.HexColor("#BBBBBB");
    public Color GeometryS = Util.HexColor("#3A3A3A");
    public Color Point = Util.HexColor("#333333");
    public Color Line = Util.HexColor("#666666");
    public Color Plane = Util.HexColor("#DDDDDD");
    public Color ElementH = Util.HexColor("#222222");
    public Color Sign = Util.HexColor("#333333");
    public Color Gizmo = Util.HexColor("#666666");
    public Color Grid = Util.HexColor("#CCCCCC");
    public Color AxisX = Util.HexColor("#F45964");
    public Color AxisY = Util.HexColor("#6ACB7F");
    public Color AxisZ = Util.HexColor("#44C0FA");

    public Color NavAxisX = Util.HexColor("#F45964");
    public Color NavAxisXS = Util.HexColor("#E54651");
    public Color NavAxisY = Util.HexColor("#6ACB7F");
    public Color NavAxisYS = Util.HexColor("#59BE6E");
    public Color NavAxisZ = Util.HexColor("#44C0FA");
    public Color NavAxisZS = Util.HexColor("#32B1EC");
    public Color NavAxisW = Util.HexColor("#DDDDDD");
    public Color NavAxisWS = Util.HexColor("#CCCCCC");
    public Color NavAxisH = Util.HexColor("#090909");
    public Color Center = Util.HexColor("#EEEEEE");
    public Color CenterS = Util.HexColor("#BBBBBB");

    public Material GeometryMaterial;
    public Material GizmosLineMaterial;
    public Material GridMaterial;
    public Material AxisMaterial;
    public Material ArrowMaterial;
    public Material SignMaterial;
    public Material SignBackgroundMaterial;
    public Material NavAxisMaterial;
    public Material CenterMaterial;

}