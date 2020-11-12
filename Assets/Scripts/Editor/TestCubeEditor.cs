using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestCube))]
public class TestCubeEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestCube t = (TestCube)target;

        if (GUILayout.Button("Rotate"))
        {
            t.Rotate();
        }

		if (GUILayout.Button("World to Local"))
		{
			t.WorldToLocal();
		}

        if (GUILayout.Button("Init"))
		{
			t.Init();
		}

        if (GUILayout.Button("Bottom Length"))
		{
			t.AddBottomLength();
		}

        if (GUILayout.Button("Bottom Angle"))
		{
			t.AddBottomAngle();
		}

        if (GUILayout.Button("Clear Conditions"))
		{
			t.ClearConditions();
		}

    }
}
