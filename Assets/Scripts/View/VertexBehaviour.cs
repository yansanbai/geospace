using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VertexBehaviour : ElementBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    const float POINT_RADIUS = 0.1f;
    const float POINT_COLLIDER_SIZE = 0.3f;

    const float FIXED_SIZE = 0.5f;
    const float UNFIXED_SIZE = 0.7f;


    public Action<Ray> OnDragVertex;
    private static Mesh mesh;

    private GeoCamera geoCamera;
    private GeoController geoController;
    private MeshRenderer meshRenderer;
    private GeoVertex geoVertex;
    private Vertex vertex;

    public void Init(GeoVertex geoVertex, GeoCamera camera)
    {
        geoCamera = camera;
        geoCamera.OnRotate += OnCameraRotate;

        geoController = GameObject.Find("/GeoController").GetComponent<GeoController>();

        this.geoVertex = geoVertex;

        if (mesh == null)
            mesh = PointMesh();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        BoxCollider meshCollider = gameObject.AddComponent<BoxCollider>();
        meshCollider.size = new Vector3(POINT_COLLIDER_SIZE, POINT_COLLIDER_SIZE, POINT_COLLIDER_SIZE);

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = ConfigManager.VertexStyle[0].Material;

        SetSize(geoVertex.isFixed ? FIXED_SIZE : UNFIXED_SIZE);

        SetColorIndex(0);
        SetStyleIndex(0);

        StyleManager.OnStyleChange += () =>
        {
            SetColorIndex(0);
            SetStyleIndex(0);
        };

        visiable = true;
    }

    public void OnDestroy()
    {
        geoCamera.OnRotate -= OnCameraRotate;
    }

    public void SetData(Vertex v)
    {
        vertex = v;
        transform.position = vertex.Point;

        OnCameraRotate();
    }

    public override void SetColorIndex(int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetPointProperty(meshRenderer, colorIndex);
    }

    public override void SetStyleIndex(int index)
    {
        base.SetStyleIndex(index);
        meshRenderer.sharedMaterial = ConfigManager.VertexStyle[styleIndex].Material;
    }

    private void SetSize(float size)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Size", size);
        meshRenderer.SetPropertyBlock(prop);
    }

    private void OnCameraRotate()
    {
        // Rotation Face to Camera
        Quaternion q = Quaternion.LookRotation(geoCamera.transform.forward, geoCamera.transform.up);
        transform.rotation = q;
    }

    private Mesh PointMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = POINT_RADIUS * new Vector3(-1, -1, 0);
        vertices[1] = POINT_RADIUS * new Vector3(-1, 1, 0);
        vertices[2] = POINT_RADIUS * new Vector3(1, 1, 0);
        vertices[3] = POINT_RADIUS * new Vector3(1, -1, 0);

        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(1, 1);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(0, 1);

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        return mesh;
    }

    //
    // private Vector3 screenPoint;
    private Vector3 offset;
    private float durationThreshold = 0.2f;
    private float timePressStarted;
    private bool isPointerDown = false;
    private bool longPressTriggered = false;

    void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (!geoVertex.isFixed && Time.time - timePressStarted > durationThreshold)
            {
                if (geoController.IsCameraRotate())
                {
                    isPointerDown = false;
                    return;
                }
                // Debug.Log("long press!");
                longPressTriggered = true;
                LongPressStart();
                // OnActive();
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("--------Point Down!");
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Debug.Log("--------Point Up!");
        if (longPressTriggered)
        {
            isPointerDown = false;
            longPressTriggered = false;
            LongPressEnd();
        }
        else if (isPointerDown)
        {
            isPointerDown = false;
            OnClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!longPressTriggered)
            OnHover(false);
    }

    public void OnDrag(PointerEventData data)
    {
        if (!longPressTriggered)
        {
            isPointerDown = false;
            return;
        }

        if (OnDragVertex != null)
            OnDragVertex(Camera.main.ScreenPointToRay(data.position));
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (!longPressTriggered)
            return;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (!longPressTriggered)
            return;
    }

    private void LongPressStart()
    {
        geoController.MoveVertexOperation(geoVertex, this);
    }

    private void LongPressEnd()
    {
        geoController.EndOperation();
    }

    private void OnClick()
    {
        geoController.ClickVertex(geoVertex);
    }

    private void OnHover(bool hover)
    {
        geoController.HoverVertex(geoVertex, hover);
    }

    public void SetHighlight(bool highlight)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Highlight", highlight ? 1 : 0);
        meshRenderer.SetPropertyBlock(prop);
    }

    public void SetActive(bool active)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(prop);
        prop.SetFloat("_Active", active ? 1 : 0);
        meshRenderer.SetPropertyBlock(prop);
    }

}
