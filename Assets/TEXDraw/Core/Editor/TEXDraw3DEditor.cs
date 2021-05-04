using TexDrawLib;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TEXDraw3D))]
[CanEditMultipleObjects]
public class TEXDraw3DEditor : Editor
{
    private SerializedProperty m_Text;
    private SerializedProperty m_FontIndex;
    private SerializedProperty m_Size;
    private SerializedProperty m_FontSize;
    private SerializedProperty m_Align;
    private SerializedProperty m_Color;
    private SerializedProperty m_Material;
    private SerializedProperty m_Fitting;
    private SerializedProperty m_Wrapping;
    private SerializedProperty m_Filling;

    private SerializedProperty m_debugReport;
    //static bool foldExpand = false;

    // Use this for initialization
    private void OnEnable()
    {
        m_Text = serializedObject.FindProperty("m_Text");
        m_FontIndex = serializedObject.FindProperty("m_FontIndex");
        m_Size = serializedObject.FindProperty("m_Size");
        m_FontSize = serializedObject.FindProperty("m_FontSize");
        m_Align = serializedObject.FindProperty("m_Align");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_Fitting = serializedObject.FindProperty("m_AutoFit");
        m_Wrapping = serializedObject.FindProperty("m_AutoWrap");
        m_Filling = serializedObject.FindProperty("m_AutoFill");
        m_debugReport = serializedObject.FindProperty("debugReport");
        Undo.undoRedoPerformed += Redraw;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Redraw;
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        TEXBoxHighlighting.DrawText(m_Text);

        if (serializedObject.targetObjects.Length == 1)
        {
            if (m_debugReport.stringValue != System.String.Empty)
                EditorGUILayout.HelpBox(m_debugReport.stringValue, MessageType.Warning);
        }

        EditorGUILayout.PropertyField(m_Size);
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(m_FontSize);
            TEXSharedEditor.DoFontIndexSelection(m_FontIndex);
            TEXSharedEditor.DoTextAligmentControl(EditorGUILayout.GetControlRect(), m_Align);
            EditorGUILayout.PropertyField(m_Color);
            TEXSharedEditor.DoMaterialGUI(m_Material, (ITEXDraw)target);
            EditorGUILayout.PropertyField(m_Fitting);
            if (!m_Fitting.hasMultipleDifferentValues && m_Fitting.enumValueIndex > 0 && !((TEXDraw3D)target).GetComponent<RectTransform>())
                EditorGUILayout.HelpBox("Fitting is useful when RectTransform is attached in this GameObject", MessageType.Info);
            EditorGUILayout.PropertyField(m_Wrapping);
            EditorGUILayout.PropertyField(m_Filling);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
            Redraw();
    }

    public void Redraw()
    {
        foreach (TEXDraw3D i in (serializedObject.targetObjects))
        {
            i.Redraw();
            i.Repaint();
        }
    }

    [MenuItem("GameObject/3D Object/TEXDraw 3D", false, 3300)]
    private static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        TEXPreference.Initialize();
        // Create a custom game object
        GameObject go = new GameObject("TEXDraw 3D");
        go.AddComponent<TEXDraw3D>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
