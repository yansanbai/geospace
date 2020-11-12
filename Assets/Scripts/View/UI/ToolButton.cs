using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnClick;
    public Action OnEnter;
    public Action OnExit;

    RectTransform rectTransform;
    Button button;
    Image image;

    Color highlightColor;

    bool isHighlight;

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();

        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);

        image = transform.Find("Image").GetComponent<Image>();

    }

    public void SetIcon(Sprite icon)
    {
        image.sprite = icon;
    }

    public void SetTintColor(Color color)
    {
        highlightColor = color;
        image.color = color;
        RefreshColor();
        // ColorBlock colorBlock = button.colors;
        // colorBlock.highlightedColor = color;
        // button.colors = colorBlock;
    }

    public void SetPosXAndWidth(float posX, float width)
    {
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, width);
    }

    public float GetPosX()
    {
        return rectTransform.offsetMin.x;
    }

    public void HandleClick()
    {
        if (OnClick != null)
            OnClick();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        isHighlight = true;
        RefreshColor();
        if (OnEnter != null)
            OnEnter();
    }

    public void OnPointerExit(PointerEventData data)
    {
        isHighlight = false;
        RefreshColor();
        if (OnExit != null)
            OnExit();
    }

    private void RefreshColor()
    {
        image.color = isHighlight ? highlightColor : StyleManager.Default;
    }
}
