using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    private ToolPanel toolPanel;

    private bool isGeometry;
    private GeometryType toolType;

    List<ToolGroup> totalToolGroups;

    public void Init(ToolPanel toolPanel)
    {
        this.toolPanel = toolPanel;
        totalToolGroups = ConfigManager.ToolGroups;
    }

    public void SetIsGeometry(bool isGeometry)
    {
        this.isGeometry = isGeometry;
    }

    public void SetToolType(GeometryType toolType)
    {
        this.toolType = toolType;
    }

    public void RefreshToolPanel()
    {
        List<ToolGroup> currentToolGroups = new List<ToolGroup>();

        foreach (ToolGroup toolGroup in totalToolGroups)
        {
            if (!isGeometry && toolGroup.Type != ToolGroupType.Geometry)
                continue;

            ToolGroup filterToolGroup = new ToolGroup();
            filterToolGroup.Icon = toolGroup.Icon;
            filterToolGroup.Name = toolGroup.Name;
            filterToolGroup.Color = toolGroup.Color;
            filterToolGroup.Type = toolGroup.Type;
            filterToolGroup.Tools = new List<Tool>();

            foreach (Tool tool in toolGroup.Tools)
            {
                if (tool.Type == toolType || (tool.Type == GeometryType.General && toolType != GeometryType.ResolvedBody && toolType != GeometryType.Function) || tool.Type == GeometryType.Common)
                    filterToolGroup.Tools.Add(tool);
            }
            if (filterToolGroup.Tools.Count > 0)
                currentToolGroups.Add(filterToolGroup);

            // Debug.Log(filterToolGroup.Name + " ------------> ");
            // foreach (Tool tool in filterToolGroup.Tools)
            //     Debug.Log(tool.Name);
            // Debug.Log(filterToolGroup.Name + " <------------ ");
        }

        toolPanel.SetToolGroups(currentToolGroups);
        toolPanel.RefreshButtonGroups();
    }
}
