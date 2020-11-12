using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonGroupState
{
    Open,
    Close,
}

public class ButtonGroup : MonoBehaviour
{

    float buttonsWidth;

    public ToolButton ToolButtonPrefab;
    public Action OnStateChange;

    public Func<int> CountOfButtons;
    public Action<ToolButton, int> ButtonAtIndex;

    ButtonGroupState state;
    RectTransform rectTransform;
    GameObject btnOpenObject;
    GameObject btnCloseObject;
    GameObject btnWrap;
    Button btnOpen;
    Button btnClose;
    List<ToolButton> buttons;


    static ObjectPool<ToolButton> buttonPool;
    static GameObject buttonPoolObject;

    public void Init()
    {
        // Object Pool
        if (buttonPool == null)
        {
            buttonPool = new ObjectPool<ToolButton>(CreateToolButton, ResetToolButton);
            buttonPoolObject = new GameObject("buttonPool");
            buttonPoolObject.transform.position = Vector3.zero;
            buttonPoolObject.SetActive(false);
            buttonPoolObject.transform.SetParent(transform);
        }

        buttons = new List<ToolButton>();

        rectTransform = GetComponent<RectTransform>();

        btnOpenObject = transform.Find("ButtonOpen").gameObject;
        btnCloseObject = transform.Find("ButtonClose").gameObject;

        btnOpen = btnOpenObject.GetComponent<Button>();
        btnClose = btnCloseObject.GetComponent<Button>();

        btnWrap = transform.Find("Buttons").gameObject;

        btnOpen.onClick.AddListener(OpenButtonClicked);
        btnClose.onClick.AddListener(CloseButtonClicked);

        SetState(ButtonGroupState.Open);
    }

    public void RefreshButtons()
    {
        int count = CountOfButtons != null ? CountOfButtons() : 0;

        for (int i = count; i < buttons.Count; i++)
            buttonPool.PutObject(buttons[i]);
        int removeCount = buttons.Count - count;
        if (removeCount >= 0)
            buttons.RemoveRange(count, removeCount);
        else
            for (int i = removeCount; i < 0; i++)
                buttons.Add(null);


        for (int i = 0; i < count; i++)
        {
            ToolButton button = buttons[i];
            if (button == null)
            {
                button = buttonPool.GetObject();
                buttons[i] = button;
            }
            ToolButtonAtIndex(button, i);
        }

        buttonsWidth = UIConstants.ToolButtonWidth * count + UIConstants.ToolButtonSpacing * (count + 1);
    }

    private void ToolButtonAtIndex(ToolButton toolButton, int i)
    {
        toolButton.transform.SetParent(btnWrap.transform, false);

        float posX = UIConstants.ToolButtonWidth * i + UIConstants.ToolButtonSpacing * (i + 1);
        float width = UIConstants.ToolButtonWidth;
        toolButton.SetPosXAndWidth(posX, width);

        ButtonAtIndex(toolButton, i);
    }

    public ToolButton CreateToolButton()
    {
        GameObject buttonObject = GameObject.Instantiate(ToolButtonPrefab.gameObject);
        buttonObject.transform.SetParent(buttonPoolObject.transform, false);
        ToolButton toolButton = buttonObject.GetComponent<ToolButton>();
        toolButton.Init();

        return toolButton;
    }

    public void ResetToolButton(ToolButton toolButton)
    {
        toolButton.transform.SetParent(buttonPoolObject.transform, false);
    }

    public void SetIcon(Sprite sprite)
    {
        Image image = btnOpen.transform.Find("Icon").GetComponent<Image>();
        image.sprite = sprite;
    }

    public void SetTintColor(Color color)
    {
        Image imageOpen = btnOpenObject.GetComponent<Image>();
        imageOpen.color = color;
        Image imageClose = btnClose.GetComponent<Image>();
        imageClose.color = color;

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetTintColor(color);
        }
    }

    public void SetPosX(float x)
    {
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, rectTransform.rect.width);
    }

    public float GetWidth()
    {
        if (state == ButtonGroupState.Open)
            return UIConstants.ToolCloseButtonWidth + buttonsWidth;

        return UIConstants.ToolOpenButtonWidth;
    }

    public float GetPosX()
    {
        return rectTransform.offsetMin.x;
    }
    void OpenButtonClicked()
    {
        SetState(ButtonGroupState.Open);
    }

    void CloseButtonClicked()
    {
        SetState(ButtonGroupState.Close);
    }

    void SetState(ButtonGroupState s)
    {
        state = s;
        btnWrap.SetActive(state == ButtonGroupState.Open);
        btnOpenObject.SetActive(state == ButtonGroupState.Close);
        btnCloseObject.SetActive(state == ButtonGroupState.Open);

        if (OnStateChange != null)
            OnStateChange();

    }
}
