using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LengthBehaviour : GizmoBehaviour
{
    const float TEXT_OFFSET = 0.1f;

    private Vector3 center;
    private Vector3 vertex1;
    private Vector3 vertex2;

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

    public void SetData(Vector3 center, Vector3 vertex1, Vector3 vertex2, float length)
    {
        this.center = center;
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;

        SetText(length);
        OnCameraRotate();
    }
    private void OnCameraRotate()
    {
        Vector3 position = (vertex1 + vertex2) / 2;
        Vector3 direction = (position - center).normalized;
        position = position + TEXT_OFFSET * direction;

        transform.position = PositionOfDirection(position, direction);
    }

    public void SetText(float length)
    {
        textMesh.text = length.ToString(UIConstants.LengthFormat);
    }

}
