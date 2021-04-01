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

    public void ChangeData(Vector3[] positions) {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public void ChangeColor(int i) {
        if (i == 1)
        {
            lineRenderer.startColor = new Color(1f, 0.8392157f, 0.4627451f, 1);
            lineRenderer.endColor = new Color(1f, 0.8392157f, 0.4627451f, 1);
        }
        else {
            lineRenderer.startColor = new Color(0.5f, 0.5f, 0.5f, 1);
            lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }
}
