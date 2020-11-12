using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Overlay : MonoBehaviour, IPointerClickHandler
{
    public Action OnClick;
    private Image image;
    private bool isActive;

    public void Init()
    {
        image = GetComponent<Image>();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        image.raycastTarget = active;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!isActive)
            return;

        if (OnClick != null)
            OnClick();
    }
}
