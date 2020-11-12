using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CircleBehaviour : ElementBehaviour
{
    private GeoCircle geoCircle;
    private Circle circle;
    public int Segments = 32;   // Segment count
    private GeometryBehaviour geometryBehaviour;

    public void Init(GeoCircle geoCircle)
    {
        this.geoCircle = geoCircle;
        visiable = true;
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
    }

    public void SetData(Circle c)
    {
        circle = c;
        CircleMesh(circle);
    }

    private void CircleMesh(Circle circle)
    {
        float Radius = circle.radius;
        int segments = Segments;
        Vector3 vertice = circle.Vertice;

        int vertices_count = Segments + 1;
        Vector3[] vertices = new Vector3[vertices_count];
        vertices[0] = vertice;
        float angledegree = 360.0f;
        float angleRad = Mathf.Deg2Rad * angledegree;
        float angleCur = angleRad;
        float angledelta = angleRad / Segments;
        VertexSpace[] vertexs = new VertexSpace[vertices_count - 1];

        for (int i = 1; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);
            if (circle.direction == CircleDirection.X)
            {
                vertices[i] = new Vector3(vertice.x, vertice.y + Radius * cosA, vertice.z + Radius * sinA);
            }
            else if (circle.direction == CircleDirection.Y)
            {
                vertices[i] = new Vector3(vertice.x + Radius * cosA, vertice.y, vertice.z + Radius * sinA);
            }
            else if (circle.direction == CircleDirection.Z)
            {
                vertices[i] = new Vector3(vertice.x + Radius * cosA, vertice.y + Radius * sinA, vertice.z);
            }
            vertexs[i - 1] = new VertexSpace(vertices[i]);
            angleCur -= angledelta;
        }

        VertexSpace v1;
        VertexSpace v2;
        GeoEdge edge;
        for (int i = 1; i < vertices_count - 1; i++)
        {
            v1 = new VertexSpace(vertices[i]);
            v2 = new VertexSpace(vertices[i + 1]);
            edge = new GeoEdge(v1, v2, false);
            geometryBehaviour.AddElement(edge);
        }
        v1 = new VertexSpace(vertices[1]);
        v2 = new VertexSpace(vertices[vertices_count - 1]);
        edge = new GeoEdge(v1, v2, false);
        geometryBehaviour.AddElement(edge);

        if (circle.displayFace)
        {
            GeoFace geoFace = new GeoFace(vertexs, false, circle.faceType);
            geometryBehaviour.AddElement(geoFace);
        }
    }
}