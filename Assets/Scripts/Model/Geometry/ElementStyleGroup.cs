using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ElementStyle
{
    public Sprite Icon;
    public Material Material;
}

[Serializable]
public class ElementStyleGroup : ScriptableObject
{
    public List<ElementStyle> VertexStyle;
    public List<ElementStyle> EdgeStyle;
    public List<ElementStyle> FaceStyle;
}
