using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ConfigManager))]
public class ConfigManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Create Tool Asset"))
        {
            CreateTool();
        }

        if (GUILayout.Button("Create Element Style Asset"))
        {
            CreateElementStyle();
        }

    }

    public void CreateTool()
    {
        ScriptableObject tool = ScriptableObject.CreateInstance<ToolBar>();

        SaveConfigsAsset(tool, "Tool");
    }

    public void CreateElementStyle()
    {
        ScriptableObject elementStyle = ScriptableObject.CreateInstance<ElementStyleGroup>();

        SaveConfigsAsset(elementStyle, "ElementStyle");
    }

    private void SaveConfigsAsset(ScriptableObject so, string name)
    {
        if (!so)
        {
            Debug.LogWarning(name + " not found");
            return;
        }

        string path = Application.dataPath + "/Configs";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = string.Format("Assets/Configs/" + name + ".asset");

        ProjectWindowUtil.CreateAsset(so, path);
    }

}
