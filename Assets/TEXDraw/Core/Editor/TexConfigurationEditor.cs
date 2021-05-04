using TexDrawLib;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TEXConfiguration))]
[CanEditMultipleObjects]
public class TEXConfigurationEditor : Editor
{
    private static string[] m_excludes = new string[] {
        "m_Script", "Typeface_Number", "Typeface_Capitals", "Typeface_Small",
        "Typeface_Commands", "Typeface_Text", "Typeface_Unicode"
        };

    private static GUIContent[] fontSets;

    private static GUIContent[] labels = new GUIContent[] { new GUIContent("Number"), new GUIContent("Capitals"), new GUIContent("Small"), new GUIContent("Commands"), new GUIContent("Text"), new GUIContent("Unicode") };

    private static int[] fontValues;

    private void OnEnable()
    {
        Undo.undoRedoPerformed += Repaint;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (OnInspectorGUI_Draw())
            TEXPreference.main.CallRedraw();
    }

    public bool OnInspectorGUI_Draw()
    {
        EditorGUI.BeginChangeCheck();
        DrawPropertiesExcluding(serializedObject, m_excludes);
        var obj = (TEXConfiguration)target;

        if (fontSets == null)
        {
            fontSets = new GUIContent[TEXPreference.main.fonts.Length];
            fontValues = new int[fontSets.Length];
            for (int i = 0; i < fontSets.Length; i++)
            {
                fontSets[i] = new GUIContent(TEXPreference.main.fonts[i].id);
                fontValues[i] = i;
            }
        }
        EditorGUILayout.Space();
        GUILayout.Label("Math Typefaces", EditorStyles.boldLabel);
        Undo.RecordObject(obj, "Modify Math Typeface");
        EditorGUI.BeginChangeCheck();

        obj.Typeface_Number = EditorGUILayout.IntPopup(labels[0], obj.Typeface_Number, fontSets, fontValues);
        obj.Typeface_Capitals = EditorGUILayout.IntPopup(labels[1], obj.Typeface_Capitals, fontSets, fontValues);
        obj.Typeface_Small = EditorGUILayout.IntPopup(labels[2], obj.Typeface_Small, fontSets, fontValues);
        obj.Typeface_Commands = EditorGUILayout.IntPopup(labels[3], obj.Typeface_Commands, fontSets, fontValues);
        obj.Typeface_Text = EditorGUILayout.IntPopup(labels[4], obj.Typeface_Text, fontSets, fontValues);
        obj.Typeface_Unicode = EditorGUILayout.IntPopup(labels[5], obj.Typeface_Unicode, fontSets, fontValues);

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(obj);

        serializedObject.ApplyModifiedProperties();

        return EditorGUI.EndChangeCheck();
    }
}
