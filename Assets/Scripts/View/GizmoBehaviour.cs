using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GizmoBehaviour : MonoBehaviour
{
    const float LINE_RADIUS = 0.02f;
    const float TEXT_BIAS = 0.2f;

    protected static Mesh lineMesh;

    protected GeoCamera geoCamera;

    public virtual void Init(GeoCamera camera)
    {
        geoCamera = camera;

        if (lineMesh == null)
            lineMesh = LineMesh();
    }

    protected GameObject InitLine(int num, out MeshRenderer meshRenderer)
    {
        GameObject line = new GameObject("line" + num);
        line.transform.SetParent(transform);
        MeshFilter meshFilter = line.AddComponent<MeshFilter>();
        meshRenderer = line.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = lineMesh;

        return line;
    }

    protected Quaternion RotationOfDirection(Vector3 direction)
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

    protected Vector3 PositionOfDirection(Vector3 origin, Vector3 direction)
    {
        direction = geoCamera.transform.InverseTransformDirection(direction);
        direction.z = 0;
        direction = geoCamera.transform.TransformDirection(direction.normalized);

        return origin + TEXT_BIAS * direction;
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
