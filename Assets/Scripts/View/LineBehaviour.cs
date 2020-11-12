using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBehaviour : GizmoBehaviour
{
    const float LINE_WIDTH = 0.05f;
    const float LINE_LENGTH = 0.2f;

    private LineRenderer lineRenderer;

    private Vector3 vertex1;
    private Vector3 vertex2;
    private Vector3 vertex3;

    public void Init()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = LINE_WIDTH;
        lineRenderer.endWidth = LINE_WIDTH;
        lineRenderer.positionCount = 3;
    }

    public void SetData(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        vertex2 = v1 + (v2 - v1) * LINE_LENGTH;
        vertex3 = v1 + (v3 - v1) * LINE_LENGTH;
        vertex1 = v1 + vertex2 + vertex3;

        lineRenderer.SetPosition(0, vertex2);
        lineRenderer.SetPosition(1, vertex1);
        lineRenderer.SetPosition(2, vertex3);
    }
}
