using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ToolGroupType
{
    Geometry,
    Condition,
    Auxiliary,
    Measure
}

[Serializable]
public class Tool
{
    public Sprite Icon;
    public string Name;
    public string Description;
    public GeometryType Type;

}

[Serializable]
public class ToolGroup
{
    public Sprite Icon;
    public string Name;
    public int Color;
    public ToolGroupType Type;
    public List<Tool> Tools;
}

[Serializable]
public class ToolBar : ScriptableObject
{
    public ToolGroup GeometryGroup;
    public ToolGroup ConditionGroup;
    public ToolGroup AuxiliaryGroup;
    public ToolGroup MeasureGroup;
}