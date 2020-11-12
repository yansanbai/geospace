using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{

    public ToolBar ToolBar;
    public ElementStyleGroup ElementStyleGroup;

    public static List<ToolGroup> ToolGroups;

    public static List<ElementStyle> VertexStyle;
    public static List<ElementStyle> EdgeStyle;
    public static List<ElementStyle> FaceStyle;

    #region Singleton
    private static ConfigManager _instance = null;
    public static ConfigManager instance
    {
        get
        {
            if (!_instance)
            {
                // check if available
                _instance = FindObjectOfType(typeof(ConfigManager)) as ConfigManager;

                // create a new one
                if (!_instance)
                {
                    var obj = new GameObject("ToolManager");
                    _instance = obj.AddComponent<ConfigManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    void Start()
    {
        if (ToolBar)
            SetTool(ToolBar);

        if (ElementStyleGroup)
            SetElementStyle(ElementStyleGroup);

    }

    #endregion

    private void SetTool(ToolBar t)
    {
        ToolBar = t;

        ToolGroups = new List<ToolGroup>();

        ToolGroups.Add(ToolBar.GeometryGroup);
        ToolGroups.Add(ToolBar.ConditionGroup);
        ToolGroups.Add(ToolBar.AuxiliaryGroup);
        ToolGroups.Add(ToolBar.MeasureGroup);

    }

    public static ToolGroup GeometryGroup()
    {
        return instance.ToolBar.GeometryGroup;
    }

    public static ToolGroup ConditionGroup()
    {
        return instance.ToolBar.ConditionGroup;
    }

    public static ToolGroup AuxiliaryGroup()
    {
        return instance.ToolBar.AuxiliaryGroup;
    }

    public static ToolGroup MeasureGroup()
    {
        return instance.ToolBar.MeasureGroup;
    }


    private void SetElementStyle(ElementStyleGroup es)
    {
        ElementStyleGroup = es;

        VertexStyle = ElementStyleGroup.VertexStyle;
        EdgeStyle = ElementStyleGroup.EdgeStyle;
        FaceStyle = ElementStyleGroup.FaceStyle;
    }
}
