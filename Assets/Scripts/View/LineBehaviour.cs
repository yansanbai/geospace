using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBehaviour : ElementBehaviour
{
    const float LINE_WIDTH = 0.03f;
    const float LINE_LENGTH = 0.2f;


    private LineRenderer lineRenderer;
    private Vector3[] positions;

    private Vector3 vertex1;
    private Vector3 vertex2;
    private Vector3 vertex3;

    public void Init(GeoLine geoLine, GeoCamera geoCamera)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = LINE_WIDTH;
        lineRenderer.endWidth = LINE_WIDTH;
        Color color = new Color(Random.Range(0.0f,1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f),1f);
        lineRenderer.startColor = new Color(0.5f,0.5f,0.5f,1);
        lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 1);
        positions = geoLine.Position();
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
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
