using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum SignBoardState
{
    Highlight,
    Error,
    Valid,
}

public class SignBoard : MonoBehaviour
{

    public Func<string, bool> OnValidate;
    public Action<string> OnInputChanged;

    SignBoardState state;

    Image frame;
    InputElement inputElement;

    string sign;

    public void Init()
    {
        frame = transform.Find("Frame").GetComponent<Image>();

        SetState(SignBoardState.Highlight);

        StyleManager.OnStyleChange += () =>
        {
            SetTintColor();
        };

        InitInputElement();
    }

    public void InitInputElement()
    {
        inputElement = transform.Find("InputElement").GetComponent<InputElement>();
        inputElement.Init(1);

        RectTransform inputRect = inputElement.GetComponent<RectTransform>();
        inputRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIConstants.InputSpacing * 2, inputRect.sizeDelta.x);
        RectTransform boardRect = GetComponent<RectTransform>();
        boardRect.sizeDelta = new Vector2(UIConstants.InputSpacing * 4 + inputElement.PreferredWidth(), boardRect.sizeDelta.y);

        inputElement.OnInputChanged += (strings) =>
        {
            sign = strings[0];
            if (OnInputChanged != null)
                OnInputChanged(sign);
            Validate();
        };

        inputElement.SetFocus(true);
    }

    public void SetSign(string sign)
    {
        inputElement.InputVertex(sign);
    }

    private void Validate()
    {
        bool result = true;
        if (OnValidate != null)
            result = OnValidate(sign);
        SetState(result ? SignBoardState.Valid : SignBoardState.Error);
    }

    public void SetState(SignBoardState s)
    {
        state = s;
        SetTintColor();
    }

    private void SetTintColor()
    {
        switch (state)
        {
            case SignBoardState.Highlight:
                frame.color = StyleManager.Highlight;
                break;
            case SignBoardState.Error:
                frame.color = StyleManager.Error;
                break;
            case SignBoardState.Valid:
                frame.color = StyleManager.Valid;
                break;
        }
    }
}
