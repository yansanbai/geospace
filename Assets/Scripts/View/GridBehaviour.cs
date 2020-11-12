using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBehaviour : MonoBehaviour
{
    const float LINE_RADIUS = 0.02f;

    protected static Mesh lineMesh;
    protected GeoCamera geoCamera;

    private int count;
    private GameObject[] lines;

    public virtual void Init(GeoCamera camera, int size)
    {
        geoCamera = camera;
        geoCamera.OnRotate += OnCameraRotate;

        if (size <= 0)
            size = 2;
        if (size % 2 == 1)
            size += 1;

        if (lineMesh == null)
            lineMesh = LineMesh();

        count = size + 1;
        int offset = size / 2;

        lines = new GameObject[count * 2];

        // x
        for (int i = 0; i < count; i++)
        {
            MeshRenderer mr;
            GameObject line = InitLine(i, out mr);
            lines[i] = line;
            line.transform.position = new Vector3(i - offset, 0, 0);
            line.transform.localScale = new Vector3(count, 1, 1);

        }
        // z
        for (int i = 0; i < count; i++)
        {
            MeshRenderer mr;
            GameObject line = InitLine(i, out mr);
            lines[count + i] = line;
            line.transform.position = new Vector3(0, 0, i - offset);
            line.transform.localScale = new Vector3(count, 1, 1);

        }

        foreach (GameObject line in lines)
        {
            MeshRenderer mr = line.GetComponent<MeshRenderer>();
            StyleManager.SetGridMaterial(mr, count);
        }

        StyleManager.OnStyleChange += () =>
        {
            foreach (GameObject line in lines)
            {
                MeshRenderer mr = line.GetComponent<MeshRenderer>();
                StyleManager.SetGridMaterial(mr, count);
            }
        };

    }

    private void OnCameraRotate()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject line = lines[i];
            line.transform.rotation = RotationOfDirection(Vector3.forward);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject line = lines[count + i];
            line.transform.rotation = RotationOfDirection(Vector3.right);
        }
    }

    private GameObject InitLine(int num, out MeshRenderer meshRenderer)
    {
        GameObject line = new GameObject("line" + num);
        line.transform.SetParent(transform);
        MeshFilter meshFilter = line.AddComponent<MeshFilter>();
        meshRenderer = line.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = lineMesh;

        return line;
    }

    private Quaternion RotationOfDirection(Vector3 direction)
    {
        Vector3 tangent = direction.normalized;
        Vector3 cameraView = -geoCamera.transform.position.normalized;
        float distance = -Vector3.Dot(tangent, cameraView);
        Vector3 normal = (cameraView + tangent * distance).normalized;
        if (normal == Vector3.zero)
            normal = geoCamera.transform.TransformDirection(Vector3.right);
        Vector3 up = Vector3.Cross(tangent, normal);

        return Quaternion.LookRotation(normal, up);
    }

    private Mesh LineMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);
        vertices[1] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[2] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[3] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);

        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        return mesh;
    }
}
