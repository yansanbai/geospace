using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class NavCamera : MonoBehaviour
{
    public const float RATIO = 0.15f;

    Camera _camera;
    GeoCamera geoCamera;
    // Use this for initialization
    public void Init(GeoCamera camera)
    {
        _camera = GetComponent<Camera>();

        InitViewport();

        geoCamera = camera;
        geoCamera.OnRotate += SetCameraTransform;

    }

    private void InitViewport()
    {
        float geoLeft = UIConstants.NavPlaneWidth + UIConstants.PlaneSpaceing;
        float geoTop = UIConstants.ToolPlaneHeight + UIConstants.PlaneSpaceing * 2;
        float geoRight = UIConstants.StatePlaneWidth + UIConstants.PlaneSpaceing * 2;
        float geoBottom = UIConstants.InputPlaneHeight + UIConstants.PlaneSpaceing * 2;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float geoWidth = screenWidth - geoLeft - geoRight;
        float geoHeight = screenHeight - geoTop - geoBottom;

        float length = (geoWidth + geoHeight) / 2 * RATIO;

        float x = (geoLeft + geoWidth - length) / screenWidth;
        float y = (geoBottom + geoHeight - length) / screenHeight;
        float width = length / screenWidth;
        float height = length / screenHeight;

        _camera.rect = new Rect(x, y, width, height);
    }

    public void SetCameraTransform()
    {
        transform.position = geoCamera.transform.position;
        transform.eulerAngles = geoCamera.transform.eulerAngles;
    }

}
