using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EdgeBehaviour : ElementBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    const float LINE_RADIUS = 0.02f;
    const float LINE_LENGTH = 1.0f;

    const float LINE_COLLIDER_SIZE = 0.2f;
    const float LINE_COLLIDER_OFFSET = 0.2f;

    private GeoCamera geoCamera;
    private GeoController geoController;
    private Mesh mesh; // Because of vertex color use for normal
    private MeshRenderer meshRenderer;
    private BoxCollider meshCollider;
    private GeoEdge geoEdge;
    private Edge edge;
    private Vector3 normal;


    // test
    // LineRenderer lineRenderer;

    public void Init(GeoEdge geoEdge, GeoCamera camera)
    {
        geoCamera = camera;
        geoCamera.OnRotate += OnCameraRotate;

        geoController = GameObject.Find("/GeoController").GetComponent<GeoController>();

        this.geoEdge = geoEdge;

        mesh = LineMesh();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<BoxCollider>();
        meshCollider.size = new Vector3(1 - LINE_COLLIDER_OFFSET, LINE_COLLIDER_SIZE, LINE_COLLIDER_SIZE);

        meshFilter.sharedMesh = mesh;

        SetColorIndex(0);
        SetStyleIndex(0);

        StyleManager.OnStyleChange += () =>
        {
            SetColorIndex(0);
            SetStyleIndex(0);
        };

        visiable = true;

        // test
        // GameObject lineObject = new GameObject();
        // lineRenderer = lineObject.AddComponent<LineRenderer>();
        // lineObject.transform.position = Vector3.zero;
        // lineRenderer.alignment = LineAlignment.View;
        // lineRenderer.useWorldSpace = false;
        // lineRenderer.startWidth = 0.02f;
        // lineRenderer.endWidth = 0.02f;
    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    // Set Edge and Normal
    public void SetData(Edge e, Vector3 n)
    {
        edge = e;
        normal = n.normalized;

        transform.position = (edge.Vertex1 + edge.Vertex2) / 2;
        transform.localScale = new Vector3(Vector3.Distance(edge.Vertex1, edge.Vertex2), 1, 1);

        Vector3 wrapNormal = normal * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

        Color color = new Color(wrapNormal.x, wrapNormal.y, wrapNormal.z, 1);
        Color[] colors = new Color[] { color, color, color, color };
        mesh.colors = colors;

        OnCameraRotate();

        // test
        // lineRenderer.SetPosition(0, transform.position);
        // lineRenderer.SetPosition(1, transform.position + n);

    }

    public override void SetColorIndex(int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetLineProperty(meshRenderer, colorIndex);
    }

    public override void SetStyleIndex(int index)
    {
        base.SetStyleIndex(index);
        meshRenderer.sharedMaterial = ConfigManager.EdgeStyle[styleIndex].Material;
    }

    private void OnCameraRotate()
    {
        if (edge == null) {
            return;
        }
        Vector3 lineDir = edge.Vertex1 - edge.Vertex2;
        // Rotation Face to Camera
        Vector3 tangent = lineDir.normalized;
        Vector3 cameraView = -geoCamera.transform.position.normalized;
        float distance = -Vector3.Dot(tangent, cameraView);
        Vector3 normal = (cameraView + tangent * distance).normalized;
        if (normal == Vector3.zero)
            normal = geoCamera.transform.TransformDirection(Vector3.right);
        Vector3 up = Vector3.Cross(tangent, normal);

        Quaternion q = Quaternion.LookRotation(normal, up);
        transform.rotation = q;

        // Stretch UV
        distance = -Vector3.Dot(lineDir, cameraView);
        float length = (lineDir + cameraView * distance).magnitude;
        float ratio = length / LINE_LENGTH;

        Vector2[] uv = mesh.uv;
        uv[2] = new Vector2(ratio, 1);
        uv[3] = new Vector2(ratio, 0);
        mesh.uv = uv;

        // test
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (edge.CanSelected)
            geoController.ClickEdge(geoEdge);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (edge.CanSelected)
            OnHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (edge.CanSelected)
            OnHover(false);
    }

    private void OnClick()
    {
        if (edge.CanSelected)
            geoController.ClickEdge(geoEdge);
    }
    private void OnHover(bool hover)
    {
        if (edge.CanSelected)
            geoController.HoverEdge(geoEdge, hover);
    }

    public void SetHighlight(bool highlight)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Highlight", highlight ? 1 : 0);
        meshRenderer.SetPropertyBlock(prop);
    }
}
