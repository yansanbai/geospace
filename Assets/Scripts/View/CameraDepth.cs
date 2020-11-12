using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraDepth : MonoBehaviour
{
    public bool IsDepthDebug = false;
    public bool UsingDepthNormal = false;

    private Material mat;
    private bool oldUsingDepthNormal = false;
    private Camera cam;
    private Shader depthShader;
    private Shader depthNormalShader;

    void Awake()
    {
        cam = GetComponent<Camera>();
        depthNormalShader = Shader.Find("Depth/CameraDepthNormal");
        depthShader = Shader.Find("Depth/CameraDepth");

        ResetCamera();
    }

    void ResetCamera()
    {
        if (cam == null || depthNormalShader == null || depthShader == null)
        {
            Debug.LogError("res is miss");
            return;
        }

        if (UsingDepthNormal)
        {
            cam.depthTextureMode = DepthTextureMode.DepthNormals;
            mat = new Material(depthNormalShader);
            mat.SetFloat("_showNormalColors", 0);
        }
        else
        {
            cam.depthTextureMode = DepthTextureMode.Depth;
            mat = new Material(depthShader);
        }

        oldUsingDepthNormal = UsingDepthNormal;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (oldUsingDepthNormal != UsingDepthNormal)
        {
            ResetCamera();
        }

        if (mat != null & IsDepthDebug)
        {
            Graphics.Blit(source, destination, mat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}