using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBoard : MonoBehaviour
{
    public ButtonBoardCell ButtonPrefab;
    public delegate int IntDelegate();
    public delegate void ButtonIntAction(ButtonBoardCell button, int i);
    public IntDelegate CountOfButtons;
    public ButtonIntAction ButtonAtIndex;

    float buttonsWidth;
    RectTransform rectTransform;
    GameObject btnWrap;
    ButtonBoardCell[] btns;

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(0, UIConstants.ElementBottonHeight);

        btnWrap = transform.Find("Buttons").gameObject;
    }

    public void InitButtons()
    {
        int count = CountOfButtons != null ? CountOfButtons() : 0;

        btns = new ButtonBoardCell[count];

        float width = UIConstants.ElementBottonWidth;
        // float height = UIConstants.ElementBottonHeight;

        for (int i = 0; i < count; i++)
        {
            GameObject buttonObject = GameObject.Instantiate(ButtonPrefab.gameObject);
            buttonObject.transform.SetParent(btnWrap.transform, false);
            ButtonBoardCell button = buttonObject.GetComponent<ButtonBoardCell>();
            button.Init();

            float posX = UIConstants.ElementBottonWidth * i;
            button.SetPosXAndWidth(posX, width);
            // rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, width);
            // // rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height);

            ButtonAtIndex(button, i);

            btns[i] = button;
        }

        buttonsWidth = UIConstants.ElementBottonWidth * count;
        rectTransform.sizeDelta = new Vector2(buttonsWidth, UIConstants.ElementBottonHeight);

    }

    public float GetWidth()
    {
        return buttonsWidth;
    }
}
