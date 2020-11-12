using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeBehaviour : GizmoBehaviour
{
    const float TEXT_OFFSET = 0.1f;

    private Vector3 center;
    private Vector3[] vertices;

    MeshRenderer textRenderer;
    TextMesh textMesh;

    public override void Init(GeoCamera camera)
    {
        base.Init(camera);

        geoCamera.OnRotate += OnCameraRotate;


        textMesh = gameObject.AddComponent<TextMesh>();
        textRenderer = textMesh.GetComponent<MeshRenderer>();

        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        StyleManager.SetGizmoTextAttr(textMesh);
        StyleManager.SetGizmoTextMaterial(textRenderer);

        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetGizmoTextAttr(textMesh);
            StyleManager.SetGizmoTextMaterial(textRenderer);
        };

    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    public void SetData(Vector3 center, Vector3[] vertices, string area)
    {
        this.center = center;
        this.vertices = vertices;

        SetText(area);
        OnCameraRotate();
    }

    private void OnCameraRotate()
    {
        Vector3 position = Vector3.zero;
        foreach (Vector3 p in vertices)
            position += p;
        position = position / vertices.Length;

        Vector3 direction = (position - center).normalized;
        position = position + TEXT_OFFSET * direction;

        transform.position = PositionOfDirection(position, direction);
    }

    public void SetText(string area)
    {
        textMesh.text = "体积: " + area;
    }

}
