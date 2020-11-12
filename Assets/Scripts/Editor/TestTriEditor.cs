using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestTri))]
public class TestTriEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestTri t = (TestTri)target;

        if (GUILayout.Button("Calculate Top By Edge Length"))
        {
            t.DrawByEdgeLength();
        }

        if (GUILayout.Button("Calculate Top By Right Angle"))
        {
            t.DrawByRightAngle();
        }
    }
}
