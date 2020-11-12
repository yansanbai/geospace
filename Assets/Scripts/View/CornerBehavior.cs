using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerBehavior : GizmoBehaviour
{
    const float LINE_LENGTH = 0.2f;
    // const float PRE_ANGLE = 22.5f;
    const int COUNT = 8;

    private Vector3 origin;
    private Vector3 dir1;
    private Vector3 dir2;

    private Vector3[] dirs;
    private GameObject[] lines;

    public override void Init(GeoCamera camera)
    {
        base.Init(camera);

        geoCamera.OnRotate += OnCameraRotate;

        lines = new GameObject[COUNT];
        dirs = new Vector3[COUNT];

        for (int i = 0; i < COUNT; i++)
        {
            MeshRenderer mr;
            GameObject line = InitLine(i, out mr);
            lines[i] = line;

            StyleManager.SetGizmosLineMaterial(mr);

            StyleManager.OnStyleChange += () =>
            {
                StyleManager.SetGizmosLineMaterial(mr);
            };
        }
    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    public void SetData(Vector3 origin, Vector3 dir1, Vector3 dir2)
    {
        this.origin = origin;
        this.dir1 = dir1;
        this.dir2 = dir2;

        float angle = Vector3.Angle(dir1, dir2);
        int count = COUNT;


        //
        Vector3 x = dir1.normalized;
        Vector3 z = Vector3.Cross(x, dir2).normalized;
        Vector3 y = Vector3.Cross(z, x).normalized;

        Matrix4x4 matrix = new Matrix4x4(
        new Vector4(x.x, x.y, x.z, 0),
        new Vector4(y.x, y.y, y.z, 0),
        new Vector4(z.x, z.y, z.z, 0),
        new Vector4(0, 0, 0, 1));

        Vector3[] vertices = new Vector3[count + 1];

        vertices[0] = origin + dir1 * LINE_LENGTH;
        for (int i = 1; i <= count; i++)
        {
            float rad = (angle / count * i) * Mathf.Deg2Rad;
            Vector3 vec = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
            vec = matrix.MultiplyPoint(vec).normalized;
            vertices[i] = origin + vec * LINE_LENGTH;
            dirs[i - 1] = vertices[i] - vertices[i - 1];
        }
        // vertices[count] = origin + dir2 * LINE_LENGTH;

        float len = (vertices[0] - vertices[1]).magnitude;

        for (int i = 0; i < count; i++)
        {
            GameObject line = lines[i];
            line.transform.position = (vertices[i] + vertices[i + 1]) / 2;
            line.transform.localScale = new Vector3(len, 1, 1);

        }

        OnCameraRotate();
    }


    private void OnCameraRotate()
    {
        for (int i = 0; i < COUNT; i++)
        {
            lines[i].transform.rotation = RotationOfDirection(dirs[i]);
        }
    }


}
