using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpin : MonoBehaviour
{

    float RotateSpeed = 120f;
    float PreAngle = 0;
    float Radius1;
    float Radius2;
    float Radius3;
    float Radius0;
    Geometry geometry;
    VertexUnit[] vertices;
    GeometryBehaviour geometryBehaviour;
    List<GeoEdge> geoEdges;
    bool Spinning = true;

    void Update()
    {
        if (!Spinning)
            return;
        foreach (GeoEdge edge in geoEdges)
        {
            if (geometryBehaviour.ContainsEdge(edge))
            {
                geometryBehaviour.RemoveElement(edge);
            }
        }
        geoEdges.Clear();
        transform.Rotate(Vector3.up * RotateSpeed * Time.deltaTime);
        AddCurFace(transform.localEulerAngles.y);
        if (transform.localEulerAngles.y >= 360 - RotateSpeed / 10)
        {
            transform.Rotate(Vector3.up * RotateSpeed * Time.deltaTime);
            AddCurFace(360);
            geoEdges.Clear();
            Spinning = false;
            DestroyGameObject();
        }
    }

    private void AddCurFace(float Angle)
    {
        float sin = Mathf.Sin(Mathf.Deg2Rad * Angle);
        float pre_sin = Mathf.Sin(Mathf.Deg2Rad * PreAngle);
        float cos = Mathf.Cos(Mathf.Deg2Rad * Angle);
        float pre_cos = Mathf.Cos(Mathf.Deg2Rad * PreAngle);
        float X0 = Radius0 * sin;
        float Z0 = Radius0 * cos;
        float preX0 = Radius0 * pre_sin;
        float preZ0 = Radius0 * pre_cos;

        float X1 = Radius1 * sin;
        float Z1 = Radius1 * cos;
        float preX1 = Radius1 * pre_sin;
        float preZ1 = Radius1 * pre_cos;

        float X2 = Radius2 * sin;
        float Z2 = Radius2 * cos;
        float preX2 = Radius2 * pre_sin;
        float preZ2 = Radius2 * pre_cos;
        if (vertices.Length == 3)
        {
            VertexSpace v0 = new VertexSpace(X0, vertices[0].Position().y, Z0);
            VertexSpace v1 = new VertexSpace(preX0, vertices[0].Position().y, preZ0);
            VertexSpace v2 = new VertexSpace(X1, vertices[1].Position().y, Z1);
            VertexSpace v3 = new VertexSpace(preX1, vertices[1].Position().y, preZ1);
            VertexSpace v4 = new VertexSpace(X2, vertices[2].Position().y, Z2);
            VertexSpace v5 = new VertexSpace(preX2, vertices[2].Position().y, preZ2);
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v0, v2, v3, v1 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v0, v1, v5, v4 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v2, v3, v5, v4 }));
            geometryBehaviour.AddElement(new GeoEdge(v0, v1));
            geometryBehaviour.AddElement(new GeoEdge(v2, v3));
            geometryBehaviour.AddElement(new GeoEdge(v4, v5));
            addBorderLine(v0.Position(), v2.Position());
            addBorderLine(v2.Position(), v4.Position());
            addBorderLine(v4.Position(), v0.Position());
        }
        else if (vertices.Length == 4)
        {
            float X3 = Radius3 * sin;
            float Z3 = Radius3 * cos;
            float preX3 = Radius3 * pre_sin;
            float preZ3 = Radius3 * pre_cos;

            VertexSpace v0 = new VertexSpace(X0, vertices[0].Position().y, Z0);
            VertexSpace v1 = new VertexSpace(preX0, vertices[0].Position().y, preZ0);
            VertexSpace v2 = new VertexSpace(X1, vertices[1].Position().y, Z1);
            VertexSpace v3 = new VertexSpace(preX1, vertices[1].Position().y, preZ1);
            VertexSpace v4 = new VertexSpace(X2, vertices[2].Position().y, Z2);
            VertexSpace v5 = new VertexSpace(preX2, vertices[2].Position().y, preZ2);
            VertexSpace v6 = new VertexSpace(X3, vertices[3].Position().y, Z3);
            VertexSpace v7 = new VertexSpace(preX3, vertices[3].Position().y, preZ3);
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v0, v2, v3, v1 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v0, v1, v7, v6 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v2, v3, v5, v4 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v4, v5, v7, v6 }));
            geometryBehaviour.AddElement(new GeoEdge(v0, v1));
            geometryBehaviour.AddElement(new GeoEdge(v2, v3));
            geometryBehaviour.AddElement(new GeoEdge(v4, v5));
            geometryBehaviour.AddElement(new GeoEdge(v6, v7));
            addBorderLine(v0.Position(), v2.Position());
            addBorderLine(v2.Position(), v4.Position());
            addBorderLine(v4.Position(), v6.Position());
            addBorderLine(v6.Position(), v0.Position());
        }
        PreAngle = Angle;
    }

    private void addBorderLine(Vector3 vertice1, Vector3 vertice2)
    {
        GeoEdge geoEdge = new GeoEdge(new VertexSpace(vertice1), new VertexSpace(vertice2));
        geoEdges.Add(geoEdge);
        geometryBehaviour.AddElement(geoEdge);
    }

    public void GetData(Geometry geometry)
    {
        this.geometry = geometry;

        GeoVertex[] geoVertices = geometry.GeoVertices();
        vertices = new VertexUnit[geoVertices.Length];
        for (int i = 0; i < geoVertices.Length; i++)
        {
            vertices[i] = geoVertices[i].VertexUnit();
        }
        Radius0 = vertices[0].Position().z;
        Radius1 = vertices[1].Position().z;
        Radius2 = vertices[2].Position().z;
        if (vertices.Length == 4)
        {
            Radius3 = vertices[3].Position().z;
        }
        else
        {
            Radius3 = Radius0;
        }
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();

        geoEdges = new List<GeoEdge>();
    }

    void DestroyGameObject()
    {
        Type type = Type.GetType("SpinAuxiliaryTool");
        SpinAuxiliaryTool auxiliaryTool;
        if (type != null)
        {
            auxiliaryTool = (SpinAuxiliaryTool)Activator.CreateInstance(type);
            auxiliaryTool.GenerateResolvedBody(geometry);
        }
        Destroy(gameObject);
    }
}