using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraCustomDepth : MonoBehaviour
{
    public bool IsDepthDebug = false;
    public Camera srcCamera;
    public Camera depthCamera;

    private GameObject depthCameraObj;
    private RenderTexture depthTexture;

    private Shader cameraDepthShader;
    private Shader objectDepthShader;

    private Material cameraDepthMat;

    public void Init(Camera camera)
    {
        srcCamera = camera;

        depthCameraObj = new GameObject("Depth Camera");
        depthCameraObj.AddComponent<Camera>();
        depthCamera = depthCameraObj.GetComponent<Camera>();
        depthCamera.enabled = false;
        depthCamera.CopyFrom(srcCamera);
        depthCamera.backgroundColor = new Color(1, 1, 1, 1);
        depthCamera.rect = new Rect(0, 0, 1, 1);
        depthCamera.clearFlags = CameraClearFlags.SolidColor;

        depthTexture = new RenderTexture(srcCamera.pixelWidth, srcCamera.pixelHeight, 16, RenderTextureFormat.ARGB32);


        cameraDepthShader = Shader.Find("Depth/CameraCustomDepth");
        objectDepthShader = Shader.Find("Depth/ObjectDepth");

        cameraDepthMat = new Material(cameraDepthShader);
    }

    internal void OnPreRender()
    {

        depthCamera.transform.position = srcCamera.transform.position;
        depthCamera.transform.rotation = srcCamera.transform.rotation;
        depthCamera.orthographicSize = srcCamera.orthographicSize;

        // depthTexture = RenderTexture.GetTemporary(srcCamera.pixelWidth, srcCamera.pixelHeight, 16, RenderTextureFormat.ARGB32);
        // depthTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        depthCamera.targetTexture = depthTexture;
        depthCamera.RenderWithShader(objectDepthShader, "RenderType");

        Shader.SetGlobalTexture("_DepthTexture", depthTexture);
        if (cameraDepthMat != null)
            cameraDepthMat.SetTexture("_DepthTexture", depthTexture);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (cameraDepthMat != null & IsDepthDebug)
        {
            Graphics.Blit(source, destination, cameraDepthMat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

}