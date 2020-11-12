using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavAxisBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Action<Vector2> OnClick;
    public Mesh mesh;

    private Vector2 rotation;

    private MeshRenderer meshRenderer;

    public void Init(Vector2 r)
    {
        rotation = r;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        StyleManager.SetNavAxisMaterial(meshRenderer);
        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetNavAxisMaterial(meshRenderer);
        };
    }

    public void OnPointerEnter(PointerEventData data)
    {
        StyleManager.SetNavAxisHMaterial(meshRenderer);
    }

    public void OnPointerExit(PointerEventData data)
    {
        StyleManager.SetNavAxisMaterial(meshRenderer);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (OnClick != null)
            OnClick(rotation);
    }
}
