using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightBehaviour : GizmoBehaviour
{
    const float LINE_LENGTH = 0.2f;

    private Vector3 origin;
    private Vector3 dir1;
    private Vector3 dir2;

    private GameObject line1;
    private GameObject line2;


    public override void Init(GeoCamera camera)
    {
        base.Init(camera);

        geoCamera.OnRotate += OnCameraRotate;

        MeshRenderer mr1, mr2;
        line1 = InitLine(1, out mr1);
        line2 = InitLine(2, out mr2);

        StyleManager.SetGizmosLineMaterial(mr1);
        StyleManager.SetGizmosLineMaterial(mr2);
        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetGizmosLineMaterial(mr1);
            StyleManager.SetGizmosLineMaterial(mr2);
        };
    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    public void SetData(Vector3 origin, Vector3 dir1, Vector3 dir2)
    {
        this.origin = origin;

        Vector3 vertex2 = origin + dir1 * LINE_LENGTH;
        Vector3 vertex3 = origin + dir2 * LINE_LENGTH;
        Vector3 vertex1 = origin + dir1 * LINE_LENGTH + dir2 * LINE_LENGTH;

        this.dir1 = vertex2 - vertex1;
        this.dir2 = vertex3 - vertex1;

        line1.transform.position = (vertex1 + vertex2) / 2;
        line1.transform.localScale = new Vector3(LINE_LENGTH, 1, 1);

        line2.transform.position = (vertex1 + vertex3) / 2;
        line2.transform.localScale = new Vector3(LINE_LENGTH, 1, 1);

        OnCameraRotate();
    }

    private void OnCameraRotate()
    {
        line1.transform.rotation = RotationOfDirection(dir1);
        line2.transform.rotation = RotationOfDirection(dir2);
    }
}