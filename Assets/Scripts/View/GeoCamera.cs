using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class GeoCamera : MonoBehaviour
{
    private Camera _camera;
    private CameraCustomDepth cameraCustomDepth;

    public bool isClampRotation;

    public Action OnRotate;


    private float viewLeft;
    private float viewBottom;

    private float screenAspect;

    private Bounds bounds;

    public void Init()
    {
        _camera = GetComponent<Camera>();
        cameraCustomDepth = GetComponent<CameraCustomDepth>();
        bounds = new Bounds();

        InitViewport();

        cameraCustomDepth.Init(_camera);
    }

    void Update()
    {
        if (isRotateAnimated)
            AnimateRotate();
        if (isZoomAnimated)
            AnimateZoom();
        if (isMoveYAnimated)
            AnimateYMove();
        if (isMoveZAnimated)
            AnimateZMove();
        if (isCenterRAnimated)
            AnimateCenterR();
        if (isCenterZMAnimated)
            AnimateCenterZM();
        if(is2DRAnimated)
            Animate2DR();
    }

    private void InitViewport()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("In GeoCamera.cs: s_width="+screenWidth+",s_height="+screenHeight);

        int refer_width = 2560;
        int refer_height = 1440;

        float factor1 = screenWidth / refer_width;
        float factor2 = screenHeight / refer_height;
        float factor = Mathf.Min(factor1, factor2);
        Debug.Log("factor1: "+factor1+",factor2: "+factor2);

        // 在这里获取缩放后的宽度

        Debug.Log("NavPlane width:"+UIConstants.NavPlaneWidth);

        float left = (UIConstants.NavPlaneWidth + UIConstants.PlaneSpaceing)*factor;
        float top = (UIConstants.ToolPlaneHeight + UIConstants.PlaneSpaceing * 2)*factor;
        float right = (UIConstants.StatePlaneWidth + UIConstants.PlaneSpaceing * 2)*factor;
        float bottom = (UIConstants.InputPlaneHeight + UIConstants.PlaneSpaceing * 2)*factor;

        

        float x = left / screenWidth;
        float y = bottom / screenHeight;
        float width = screenWidth - left - right;
        float height = screenHeight - top - bottom;

        Debug.Log("In GeoCamera.cs: width="+width+",height="+height);

        float widthRatio = width / screenWidth;
        float heightRatio = height / screenHeight;
        _camera.rect = new Rect(x, y, widthRatio, heightRatio);

        viewLeft = left;
        viewBottom = bottom;
        screenAspect = width / height;

        // Shader.SetGlobalVector("_GeoViewport", new Vector4(x, y, viewportWidth, viewportHeight));
    }

    private bool isRotateAnimated = false;
    private bool isZoomAnimated = false;
    private bool isMoveYAnimated = false;
    private bool isMoveZAnimated = false;
    private bool isCenterRAnimated = false;
    private bool isCenterZMAnimated = false;
    private bool is2DRAnimated = false;

    private float count;
    private float iRotateY = 0;
    private float iRotateX = 0;
    private float iPositionY = 0;
    private float iPositionZ = 0;
    private float iZoom = 0;

    private const float ANIMATE_COUNT = 5;

    private bool IsAnimated()
    {
        return isRotateAnimated || isZoomAnimated || isMoveYAnimated || isMoveZAnimated || isCenterRAnimated || isCenterZMAnimated || is2DRAnimated;
    }

    // dRotateY 目标RotateY
    // dRotateX 目标RotateX
    public void TriggerRotateAnimation(float dRotateY, float dRotateX)
    {
      
        if (IsAnimated())
            return;
        isRotateAnimated = true;
        count = ANIMATE_COUNT;
        float deltaHorizontal = Math.Mod(dRotateY - rotateY + 180, 360) - 180;
        iRotateY = deltaHorizontal / count;
        iRotateX = (dRotateX - rotateX) / count;
    }

    public void TriggerZoomAnimation(float dZoom)
    {
        if (IsAnimated())
            return;

        if (dZoom > 0 && orthographic >= orthographicMax)
            return;
        else if (dZoom < 0 && orthographic <= orthographicMin)
            return;

        isZoomAnimated = true;

        dZoom = Mathf.Clamp(orthographic + dZoom, orthographicMin, orthographicMax) - orthographic;

        count = ANIMATE_COUNT;
        iZoom = dZoom / count;
    }

    public void TriggerMoveZAnimation(float dRotateY, float dRotateX, float dPositionZ)
    {
        if (IsAnimated())
            return;

        isMoveZAnimated = true;

        count = ANIMATE_COUNT;
        iPositionZ = dPositionZ / count;
        float deltaHorizontal = Math.Mod(dRotateY - rotateY + 180, 360) - 180;
        iRotateY = deltaHorizontal / count;
        iRotateX = (dRotateX - rotateX) / count;
    }

    public void TriggerMoveYAnimation(float dPositionY)
    {
        if (IsAnimated())
            return;

        isMoveYAnimated = true;

        count = ANIMATE_COUNT;
        iPositionY = dPositionY / count;
    }

    // Rotate
    public void TriggerCenterRAnimation()
    {
        if (IsAnimated())
            return;
        isCenterRAnimated = true;

        count = ANIMATE_COUNT;

        float dRotateY = defaultRotationY - rotateY;
        dRotateY = Math.Mod(dRotateY + 180, 360) - 180;
        float dRotateX = defaultRotationX - rotateX;
        iRotateY = dRotateY / count;
        iRotateX = dRotateX / count;
    }

    public void Trigger2DRAnimation()
    {
        if (is2DRAnimated)
            return;
        is2DRAnimated = true;

        count = ANIMATE_COUNT;

        float dRotateY = 0f - rotateY;
        dRotateY = Math.Mod(dRotateY + 180, 360) - 180;
        float dRotateX = 0f - rotateX;
        iRotateY = dRotateY / count;
        iRotateX = dRotateX / count;
    }

    // Zoom Move
    public void TriggerCenterZMAnimation()
    {
        if (IsAnimated())
            return;

        isCenterZMAnimated = true;

        count = ANIMATE_COUNT;
        iZoom = (defaultOrthographic - orthographic) / count;
        iPositionY = (defaultPositionY - positionY) / count;
    }

    private void AnimateRotate()
    {
        rotateY += iRotateY;
        rotateX += iRotateX;
        SetCameraAttributes();
        count--;

        if (count == 0)
            isRotateAnimated = false;
    }

    private void AnimateZoom()
    {
        orthographic += iZoom;
        SetCameraAttributes();
        count--;

        if (count == 0)
            isZoomAnimated = false;
    }

    private void AnimateYMove()
    {
        positionY += iPositionY;
        SetCameraAttributes();
        count--;

        if (count == 0)
            isMoveYAnimated = false;
    }

    private void AnimateZMove()
    {
        positionZ += iPositionZ;
        rotateY += iRotateY;
        rotateX += iRotateX;
        SetCameraAttributes();
        count--;

        if (count == 0)
            isMoveZAnimated = false;
    }

    private void AnimateCenterR()
    {
        rotateY += iRotateY;
        rotateX += iRotateX;
        
        if (positionZ != 0) {
            positionZ -= iPositionZ;
        } else if (positionZ == 0) {
            iPositionZ = 0;
        }
        SetCameraAttributes();
        count--;

        if (count == 0)
            isCenterRAnimated = false;
    }

    private void Animate2DR()
    {
        rotateY += iRotateY;
        rotateX += iRotateX;

        if (positionZ != 0)
        {
            positionZ -= iPositionZ;
        }
        else if (positionZ == 0)
        {
            iPositionZ = 0;
        }
        SetCameraAttributes();
        count--;

        if (count == 0)
            is2DRAnimated = false;
    }

    private void AnimateCenterZM()
    {
        orthographic += iZoom;
        positionY += iPositionY;
        SetCameraAttributes();
        count--;

        if (count == 0)
            isCenterZMAnimated = false;
    }

    public void Rotate(Vector2 deltaDegree)
    {
        if (IsAnimated())
            return;

        rotateY -= deltaDegree.x;
        rotateX += deltaDegree.y;

        rotateY = Math.Mod(rotateY, 360);
        rotateX = Math.Mod(rotateX, 360);

        if (isClampRotation)
            rotateX = Mathf.Clamp(rotateX, -120f, 60f);

        SetCameraAttributes();
    }

    public void Zoom(float deltaOrthographic)
    {
        if (IsAnimated())
            return;

        orthographic += deltaOrthographic;
        SetCameraAttributes();
    }

    public void Move(float deltaY)
    {
        if (IsAnimated())
            return;

        positionY += deltaY;
        SetCameraAttributes();
    }


    // private float rotateSpeed = 1f;
    // private float zoomSpeed = 1f;
    private float rotateY = 0;
    private float rotateX = 0;
    private float positionY = 0;
    private float positionZ = 0;
    private float orthographic = 0;
    private float defaultRotationY = 225f;
    private float defaultRotationX = 30f;
    private float defaultDistance = Mathf.Sqrt(2) * 6f;
    private float defaultPositionY = 0f;
    private float defaultPositionZ = 0f;
    private float defaultOrthographic = 3f;
    private float orthographicMin = 1.0f;
    private float orthographicMax = 5.0f;

    public void InitDefault()
    {
        rotateX = defaultRotationX;
        rotateY = defaultRotationY;
        positionY = defaultPositionY;
        positionZ = defaultPositionZ;
        orthographic = defaultOrthographic;

        SetCameraAttributes();
    }

    public void SetCameraAttributes()
    {
        Vector3 cameraPosition = _camera.transform.position;
        Vector3 cameraRotation = _camera.transform.eulerAngles;

        cameraRotation.x = rotateX;
        cameraRotation.y = rotateY;
        cameraRotation.z = 0;

        float rotateYRad = rotateY * Mathf.Deg2Rad;
        float rotateXRad = rotateX * Mathf.Deg2Rad;

        float deltaDistance = defaultDistance * (1 - Mathf.Cos(rotateXRad));  //rotate vertical

        cameraPosition.x = (deltaDistance - defaultDistance) * Mathf.Sin(rotateYRad);
        cameraPosition.z = (deltaDistance - defaultDistance) * Mathf.Cos(rotateYRad) + positionZ;
        cameraPosition.y = defaultDistance * Mathf.Sin(rotateXRad) + positionY;

        _camera.transform.position = cameraPosition;
        _camera.transform.eulerAngles = cameraRotation;

        orthographic = Mathf.Clamp(orthographic, orthographicMin, orthographicMax);
        _camera.orthographicSize = orthographic;

        RefreshBounds();

        OnRotate();
    }

    public void SetCameraAttributes(float rotateX, float rotateY, float rotateZ) {
        Vector3 cameraPosition = _camera.transform.position;
        Vector3 cameraRotation = _camera.transform.eulerAngles;

        cameraRotation.x = rotateX;
        cameraRotation.y = rotateY;
        cameraRotation.z = rotateZ;

        float rotateYRad = rotateY * Mathf.Deg2Rad;
        float rotateXRad = rotateX * Mathf.Deg2Rad;

        float deltaDistance = defaultDistance * (1 - Mathf.Cos(rotateXRad));  //rotate vertical

        cameraPosition.x = (deltaDistance - defaultDistance) * Mathf.Sin(rotateYRad);
        cameraPosition.z = (deltaDistance - defaultDistance) * Mathf.Cos(rotateYRad);
        cameraPosition.y = defaultDistance * Mathf.Sin(rotateXRad) + positionY;

        _camera.transform.position = cameraPosition;
        _camera.transform.eulerAngles = cameraRotation;

        orthographic = Mathf.Clamp(orthographic, orthographicMin, orthographicMax);
        _camera.orthographicSize = orthographic;

        RefreshBounds();

        OnRotate();


    }

    public bool IsInViewport()
    {
        Vector2 point = _camera.ScreenToViewportPoint(Input.mousePosition);
        return _camera.rect.Contains(point);
    }

    public Vector2 WorldToViewPoint(Vector3 position)
    {
        Vector3 point = _camera.WorldToScreenPoint(position);
        return ScreenToViewPoint(point);
    }

    private Vector2 ScreenToViewPoint(Vector2 position)
    {
        return new Vector2(position.x - viewLeft, position.y - viewBottom);
    }

    private void RefreshBounds()
    {
        float height = _camera.orthographicSize * 2;
        float width = height * screenAspect;

        float far = _camera.farClipPlane;
        float near = _camera.nearClipPlane;

        float z = (far + near) / 2;
        float length = (far - near);

        bounds.center = new Vector3(0, 0, z);
        bounds.size = new Vector3(width, height, length);
    }

    public bool IntersectRay(Ray ray, out Vector3 position)
    {
        ray.origin = transform.InverseTransformPoint(ray.origin);
        ray.direction = transform.InverseTransformDirection(ray.direction);

        float distance;
        bool result = bounds.IntersectRay(ray, out distance);
        if (result)
            position = ray.origin + ray.direction * distance;
        else
            position = ray.origin;

        position = transform.TransformPoint(position);

        return result;
    }
    public CameraCustomDepth get_CameraCustomDepth()
    {
        return cameraCustomDepth;
    }
}
