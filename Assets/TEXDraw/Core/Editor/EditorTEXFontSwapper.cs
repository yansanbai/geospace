using System;
using System.Collections.Generic;
using TexDrawLib;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class FontSwapData
{
    public int old;
    public int target;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return ((FontSwapData)obj).old == old;
    }

    public override int GetHashCode()
    {
        return old.GetHashCode();
    }
}

public class EditorTEXFontSwapper : EditorWindow
{
    public ReorderableList list;
    public List<FontSwapData> data;
    private Vector2 scr;

    private static string helpInfo =
        "This is a helper for changing font-index in TEXDraw components across the scene with few clicks." +
        "\nThis is useful in case if selected font is mistargeted after importing process, due we save them in their 'index' values (for simplicity)." +
        "\nYou can set multiple changes here. Order doesn't matter.";

    void Init()
    {
        data = new List<FontSwapData>();
        list = new ReorderableList(data, typeof(int))
        {
            headerHeight = 20f,
            elementHeight = 22f,
            drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnHeaderDraw),
            drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnElementDraw)
        };
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += Repaint;
        Init();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
    }

    private void OnGUI()
    {
        if (Screen.width > 350)
            EditorGUILayout.HelpBox(helpInfo, MessageType.Info);
        else
            EditorGUILayout.Space();
        if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            Undo.RecordObject(this, "Changes to TEX Font Swapper");
        EditorGUI.BeginChangeCheck();
        scr = EditorGUILayout.BeginScrollView(scr, GUILayout.ExpandHeight(true));
        var sample = EditorGUILayout.ObjectField("Sample From:", null, typeof(MonoBehaviour), true);
        if (sample is ITEXDraw)
        {
            var idx = ((ITEXDraw)sample).fontIndex;
            var swap = new FontSwapData() { old = idx };
            if (idx < 0)
                Debug.LogFormat("Doesn't want to pick sample from {0}, it is non sense to remap the one who already choose Math Typefaces (-1)", sample.name);
            else if (data.Contains(swap))
                Debug.LogFormat("Won't pick sample from {0}, font index number {1} is already exist", sample.name, idx);
            else
                data.Add(swap);
        }

        var r = EditorGUILayout.GetControlRect(GUILayout.Height(list.GetHeight()));
        r = EditorGUI.IndentedRect(r);
        list.DoList(r);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("APPLY to Active Scene"))
        {
            Apply(UnityEngine.Object.FindObjectsOfType<GameObject>());
        }
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("APPLY to Selected Only"))
        {
            Apply(Selection.gameObjects);
        }
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Reset", GUILayout.Height(38)))
        {
            data.Clear();
        }
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
    }

    private void Apply(GameObject[] objs)
    {
        int appliedCount = 0, texCount = 0;
        foreach (var obj in objs)
        {
            var tex = TEXEditorMenus.GetTexDraw(obj);
            if (tex == null)
                continue;
            texCount++;
            var match = data.Find(x => x.old == tex.fontIndex);
            if (match != null && match.target != match.old)
            {
                appliedCount++;
                Undo.RecordObject((Component)tex, "Applied Font Index Swapping");
                tex.fontIndex = match.target;
                EditorUtility.SetDirty((Component)tex);
            }
        }
        Debug.LogFormat("Replaced {0} from {1} TEXDraw objects in {2} GameObjects on scene", appliedCount, texCount, objs.Length);
    }

    private void OnElementDraw(Rect r, int idx, bool active, bool focus)
    {
        var d = data[idx];
        r.height = 16;
        r.y += 3;
        r.width = r.width / 2f - 2;
        d.old = EditorGUI.IntSlider(r, d.old, 0, 31);
        r.x += r.width + 4;
        d.target = EditorGUI.IntPopup(r, d.target, TEXPreference.main.FontIDsGUI, TEXPreference.main.FontIndexs);
    }

    private void OnHeaderDraw(Rect r)
    {
        GUI.Label(r, "Swap Indexes ( From -- To )");
    }
}
