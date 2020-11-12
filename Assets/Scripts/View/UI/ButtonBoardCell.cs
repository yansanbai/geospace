using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBoardCell : MonoBehaviour
{

    public Sprite NormalImage;
    public Sprite ActiveImage;
    public Sprite DefaultIcon;

    public event Action OnClick;

    public event Action<bool> OnActiveChanged;


    Button button;
    Image background;
    Image icon;
    bool isActive;

    public void Init()
    {
        button = GetComponent<Button>();
        background = GetComponent<Image>();
        icon = transform.Find("Icon").GetComponent<Image>();

        button.onClick.AddListener(() =>
        {
            if (OnClick != null)
                OnClick();
        });
    }

    public void SetIcon(Sprite image)
    {
        if (!image)
            image = DefaultIcon;
        icon.sprite = image;
    }

    public void SetColor(Color color)
    {
        icon.color = color;
    }

    public void SetPosXAndWidth(float posX, float width)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, width);

    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (ActiveImage == null || NormalImage == null)
            return;
        background.sprite = active ? ActiveImage : NormalImage;

        if (OnActiveChanged != null)
            OnActiveChanged(active);
    }

    public bool IsActive()
    {
        return isActive;
    }
}
