using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Test t = (Test)target;

        if (GUILayout.Button("f"))
        {
            Range<float> range = t.TriPyramidEdgeRange(t.a, t.b, t.c, t.d, t.e);
            Debug.Log("f: " + t.f + "  : " + range.min + " - " + range.max);

        }
        if (GUILayout.Button("e"))
        {
            Range<float> range = t.TriPyramidEdgeRange(t.b, t.a, t.c, t.d, t.f);
            Debug.Log("e: " + t.e + "  : " + range.min + " - " + range.max);

        }

        if (GUILayout.Button("d"))
        {
            Range<float> range = t.TriPyramidEdgeRange(t.c, t.a, t.b, t.e, t.f);
            Debug.Log("d: " + t.d + "  : " + range.min + " - " + range.max);

        }


        if (GUILayout.Button("ID"))
        {
            t.Id();
        }

    }
}
