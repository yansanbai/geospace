using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavCenterBehaviour : MonoBehaviour, IPointerClickHandler
{
    public Action OnClick;
    public Mesh mesh;

    private MeshRenderer meshRenderer;

    public void Init()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        StyleManager.SetCenterMaterial(meshRenderer);
        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetCenterMaterial(meshRenderer);
        };
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (OnClick != null)
            OnClick();
    }
}
