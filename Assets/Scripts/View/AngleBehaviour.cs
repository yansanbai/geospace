using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleBehaviour : GizmoBehaviour
{
    const float TEXT_OFFSET = 0.3f;
    private Vector3 origin;
    private Vector3 dir1;
    private Vector3 dir2;

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

    public void SetData(Vector3 origin, Vector3 dir1, Vector3 dir2, float angle)
    {
        this.origin = origin;
        this.dir1 = dir1;
        this.dir2 = dir2;

        SetText(angle);
        OnCameraRotate();
    }


    private void OnCameraRotate()
    {
        Vector3 direction = (dir1 + dir2).normalized;
        Vector3 position = origin + TEXT_OFFSET * direction;

        transform.position = PositionOfDirection(position, direction);
    }


    public void SetText(float angle)
    {
        textMesh.text = angle.ToString(UIConstants.AngleFormat);
    }

}
