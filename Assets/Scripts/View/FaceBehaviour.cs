using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FaceBehaviour : ElementBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    const float PLANE_COLLIDER_SIZE = 0.01f;
    const float PLANE_COLLIDER_OFFSET = 0.1f;

    private GeoController geoController;

    private Mesh mesh; // Because of vertex color use for normal
    private Mesh colliderMesh;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private GeoFace geoFace;
    private Face face;

    private bool interactable;

    public void Init(GeoFace geoFace)
    {
        geoController = GameObject.Find("/GeoController").GetComponent<GeoController>();

        this.geoFace = geoFace;

        mesh = new Mesh();
        colliderMesh = new Mesh();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = colliderMesh;

        meshRenderer.sharedMaterial = ConfigManager.FaceStyle[0].Material;

        SetColorIndex(0);
        SetStyleIndex(0);

        StyleManager.OnStyleChange += () =>
        {
            SetColorIndex(0);
            SetStyleIndex(0);
        };

        visiable = true;
        interactable = true;
    }

    public void OnDestroy()
    {
    }

    public void SetData(Face f)
    {
        face = f;

        FaceMesh(mesh, face);
        FaceColliderMesh(mesh, colliderMesh, face);

        meshCollider.enabled = true;
        meshCollider.convex = true;

        RefreshInteractable();
    }

    public override void SetColorIndex(int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetPlaneProperty(meshRenderer, colorIndex);
    }

    public override void SetStyleIndex(int index)
    {
        base.SetStyleIndex(index);
        meshRenderer.sharedMaterial = ConfigManager.FaceStyle[styleIndex].Material;
    }

    public void SetInteractable(bool i)
    {
        interactable = i;
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        meshCollider.enabled = interactable;
    }

    private void FaceMesh(Mesh mesh, Face face)
    {

        Vector3[] vertices = face.Vertices;
        List<int> meshTriangles = new List<int>();

        for (int i = 1; i < vertices.Length - 1; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i);
            meshTriangles.Add(i + 1);
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles.ToArray();

        mesh.RecalculateNormals();

        // uv
        Vector3 y = mesh.normals[0].normalized;
        Vector3 x = (vertices[1] - vertices[0]).normalized;
        Vector3 z = Vector3.Cross(x, y);

        Matrix4x4 matrix = new Matrix4x4(
            new Vector4(x.x, y.x, z.x, 0),
            new Vector4(x.y, y.y, z.y, 0),
            new Vector4(x.z, y.z, z.z, 0),
             new Vector4(0, 0, 0, 1));

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = matrix.MultiplyPoint(vertices[i]);
            uvs[i] = new Vector2(position.x, position.z);
        }
        mesh.uv = uvs;
    }

    private void FaceColliderMesh(Mesh mesh, Mesh colliderMesh, Face face)
    {
        Vector3[] faceVertices = face.Vertices;
        int count = faceVertices.Length;

        Vector3[] faceNormal = mesh.normals;
        Vector3[] vertices = new Vector3[count * 2];

        Vector3 total = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            total += faceVertices[i];
        }

        Vector3 center = total / count;

        for (int i = 0; i < count; i++)
        {
            Vector3 normal = faceNormal[i].normalized;
            Vector3 centerNormal = center - faceVertices[i];
            vertices[i * 2] = faceVertices[i] + normal * PLANE_COLLIDER_SIZE + centerNormal * PLANE_COLLIDER_OFFSET;
            vertices[i * 2 + 1] = faceVertices[i] - normal * PLANE_COLLIDER_SIZE + centerNormal * PLANE_COLLIDER_OFFSET;
        }

        List<int> meshTriangles = new List<int>();

        for (int i = 1; i < count - 1; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i * 2);
            meshTriangles.Add(i * 2 + 2);

            meshTriangles.Add(i * 2 + 3);
            meshTriangles.Add(i * 2 + 1);
            meshTriangles.Add(1);
        }

        colliderMesh.vertices = vertices;
        colliderMesh.triangles = meshTriangles.ToArray();

        colliderMesh.RecalculateNormals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (face.Canselected)
            geoController.ClickFace(geoFace);
        if (face.faceType != FaceType.Normal) 
            geoController.ClickSpreadFace(geoFace);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (face.Canselected || face.faceType != FaceType.Normal)
            OnHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (face.Canselected || face.faceType != FaceType.Normal)
            OnHover(false);
    }

    private void OnClick()
    {
        if (face.Canselected)
            geoController.ClickFace(geoFace);
    }
    private void OnHover(bool hover)
    {
        if (face.Canselected || face.faceType != FaceType.Normal)
            geoController.HoverFace(geoFace, hover);
    }

    public void SetHighlight(bool highlight)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Highlight", highlight ? 1 : 0);
        meshRenderer.SetPropertyBlock(prop);
    }

    public void SetAlpha(float alpha)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Alpha", alpha);
        meshRenderer.SetPropertyBlock(prop);
    }
}
