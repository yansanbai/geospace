using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SignState
{
    Normal,
    Highlight,
    Error,
    Valid,
}

public class SignBehaviour : MonoBehaviour
{
    const float SIGN_BIAS = 0.2f;
    const float SIGN_PADDING_WIDTH = 0.1f;
    private GeoCamera geoCamera;

    private static Mesh mesh;

    MeshRenderer textRenderer;
    TextMesh textMesh;
    GameObject backgroundObject;
    MeshRenderer backgroundRenderer;

    int id;
    string sign;
    Vector3 vertex;
    Vector3 center;

    SignState state;

    // test
    // LineRenderer lineRenderer;

    public void Init(int i, GeoCamera camera)
    {
        geoCamera = camera;
        geoCamera.OnRotate += OnCameraRotate;

        id = i;

        if (mesh == null)
            mesh = SignMesh();

        textMesh = gameObject.AddComponent<TextMesh>();
        textRenderer = textMesh.GetComponent<MeshRenderer>();

        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        StyleManager.SetSignAttr(textMesh);
        StyleManager.SetSignMaterial(textRenderer);

        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetSignAttr(textMesh);
            StyleManager.SetSignMaterial(textRenderer);

            SetText();
        };

        backgroundObject = new GameObject("background");
        backgroundObject.transform.SetParent(transform);
        backgroundObject.transform.position = Vector3.zero;

        MeshFilter meshFilter = backgroundObject.AddComponent<MeshFilter>();
        backgroundRenderer = backgroundObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = mesh;

        SetState(SignState.Normal);

        SetTintColor();
        StyleManager.OnStyleChange += () =>
        {
            SetTintColor();
        };

        //test
        // GameObject lineObject = new GameObject();
        // lineRenderer = lineObject.AddComponent<LineRenderer>();
        // lineObject.transform.position = Vector3.zero;
        // lineRenderer.alignment = LineAlignment.View;
        // lineRenderer.useWorldSpace = true;
        // lineRenderer.startWidth = 0.02f;
        // lineRenderer.endWidth = 0.02f;
    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    public void SetData(Vector3 vertex, Vector3 center)
    {
        this.vertex = vertex;
        this.center = center;

        OnCameraRotate();
    }

    private void OnCameraRotate()
    {
        Vector3 direction = (vertex - center).normalized;
        if (direction == Vector3.zero)
            direction = Vector3.forward;
        direction = geoCamera.transform.InverseTransformDirection(direction);
        direction.z = 0;
        direction = geoCamera.transform.TransformDirection(direction.normalized);

        transform.position = vertex + SIGN_BIAS * direction;

        // lineRenderer.SetPosition(0, vertex);
        // lineRenderer.SetPosition(1, vertex + SIGN_BIAS * direction);

    }

    public void SetSign(string s)
    {
        sign = s;
        SetText();
    }

    private void SetText()
    {
        int subSize = UIConstants.TextFontSubSize;
        textMesh.text = UIConstants.SignFormat(sign, subSize);

        Bounds textBounds = textRenderer.bounds;

        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        backgroundRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_ScaleX", textBounds.size.x + SIGN_PADDING_WIDTH);
        prop.SetFloat("_ScaleY", textBounds.size.y);
        backgroundRenderer.SetPropertyBlock(prop);
    }

    public void SetState(SignState s)
    {
        state = s;
        backgroundObject.SetActive(state != SignState.Normal);
        SetTintColor();
    }

    private void SetTintColor()
    {
        switch (state)
        {
            case SignState.Highlight:
                StyleManager.SetSignNMaterial(backgroundRenderer);
                break;
            case SignState.Error:
                StyleManager.SetSignEMaterial(backgroundRenderer);
                break;
            case SignState.Valid:
                StyleManager.SetSignVMaterial(backgroundRenderer);
                break;
            default:
                StyleManager.SetSignNMaterial(backgroundRenderer);
                break;
        }
    }

    private Mesh SignMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = 0.5f * new Vector3(-1, -1, 0);
        vertices[1] = 0.5f * new Vector3(-1, 1, 0);
        vertices[2] = 0.5f * new Vector3(1, 1, 0);
        vertices[3] = 0.5f * new Vector3(1, -1, 0);

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
