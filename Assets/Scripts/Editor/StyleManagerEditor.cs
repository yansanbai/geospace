using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(StyleManager))]
public class StyleManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StyleManager t = (StyleManager)target;

        if (GUILayout.Button("Create Style Asset"))
        {
            Create();
        }

         if (GUILayout.Button("Refresh Style"))
        {
            t.RefreshStyle();
        }

    }

    public void Create()
    {
        ScriptableObject style = ScriptableObject.CreateInstance<Style>();

        if (!style)
        {
            Debug.LogWarning("Style not found");
            return;
        }

        string path = Application.dataPath + "/Configs";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = string.Format("Assets/Configs/Style.asset");

        ProjectWindowUtil.CreateAsset(style, path);
    }

}
