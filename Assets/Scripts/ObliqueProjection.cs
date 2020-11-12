using UnityEngine;
using System.Collections;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class ObliqueProjection : MonoBehaviour
{
    private Camera _camera;
    
    public float angle = 45.0f;
    public float zScale = 0.5f;
    public float zOffset = 0.0f;

    public void Start()
    {
        _camera = GetComponent<Camera>();
    }

    public void Apply()
    {
        _camera.orthographic = true;
        var orthoHeight = _camera.orthographicSize;
        var orthoWidth = _camera.aspect * orthoHeight;
        var m = Matrix4x4.Ortho(-orthoWidth, orthoWidth, -orthoHeight, orthoHeight, _camera.nearClipPlane, _camera.farClipPlane);
        var s = zScale / orthoHeight;
        m[0, 2] = +s * Mathf.Sin(Mathf.Deg2Rad * -angle);
        m[1, 2] = -s * Mathf.Cos(Mathf.Deg2Rad * -angle);
        m[0, 3] = -zOffset * m[0, 2];
        m[1, 3] = -zOffset * m[1, 2];
        _camera.projectionMatrix = m;
    }

    void OnEnable()
    {
        Apply();
    }

    void OnDisable()
    {
        _camera.ResetProjectionMatrix();
    }
}