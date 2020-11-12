using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputNum : Inputable
{
    public Action<float> OnInputChanged;

    FormNum formNum;
    float num;
    Text input;
    bool isFocus;
    RectTransform rectTransform;
    RectTransform caret;
    float textWidth;
    float width;

    public void Init(FormNum formNum)
    {
        Init();
        Refresh(formNum);
    }

    public void Init()
    {
        input = transform.Find("Text").GetComponent<Text>();
        input.fontSize = (int)UIConstants.TextFontSize;

        KeyboardSystem.OnKey.AddListener(InputKey);

        rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);


        GameObject caretObject = transform.Find("Caret").gameObject;
        caret = caretObject.GetComponent<RectTransform>();
        caret.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);

        SetFocus(false);

        formNum = new FormNum();
    }

    public void Refresh(FormNum formNum)
    {
        this.formNum = formNum;
        if (formNum.num == 0 && formNum.isEmpty)
            SetText("");
        else
            SetText(formNum.ToString());

        TextChanged();
    }

    public void OnDestroy()
    {
        if (KeyboardSystem.instance)
            KeyboardSystem.OnKey.RemoveListener(InputKey);
    }

    public override void SetFocus(bool f)
    {
        isFocus = f;
        caret.gameObject.SetActive(f);
    }

    public override void OnPointerClick(PointerEventData data)
    {
        if (OnClickToFocus != null)
        {
            if (OnClickToFocus())
                SetFocus(true);
        }
    }

    private void InputKey(KeyCode keycode)
    {
        // Debug.Log("Key held down: " + keycode);
        if (!isFocus)
            return;
        if (keycode >= KeyCode.Alpha0 && keycode <= KeyCode.Alpha9)
        {
            char c = (char)(keycode - KeyCode.Alpha0 + '0');
            InputNumber(c);
            InputChanged();
        }
        else if (keycode == KeyCode.Period)
        {
            InputPeriod();
            InputChanged();
        }
        else if (keycode == KeyCode.Minus )
        {
            InputMinus();
        }
        else if (keycode == KeyCode.Equals)
        {
            InputPlus();
        }
        else if (keycode == KeyCode.Delete || keycode == KeyCode.Backspace)
        {
            InputDelete();
            InputChanged();
        }
    }

    private void InputChanged()
    {
        formNum.isEmpty = input.text == "";
        TextChanged();
    }

    private void TextChanged()
    {
        if (OnInputChanged != null)
            OnInputChanged(num);
    }

    private void InputNumber(char letter)
    {
        SetText(input.text + letter);
    }

    private void InputPeriod()
    {
        if (input.text.Contains("."))
            return;
        SetText(input.text + ".");
    }

    private void InputMinus()
    {
        if (input.text != "")
            return;
        SetText("-");
    }

    private void InputPlus()
    {
        if (input.text != "")
            return;
        SetText("+");
    }

    private void InputDelete()
    {
        int length = input.text.Length;
        if (length == 0)
            return;
        string text = input.text.Substring(0, length - 1);
        SetText(text);
    }

    private void SetText(string text)
    {
        input.text = text;

        if (text == "" || text == "-" || text == "+")
            num = 0;
        else
            num = float.Parse(text);

        textWidth = input.cachedTextGenerator.GetPreferredWidth(input.text, input.GetGenerationSettings(GetComponent<RectTransform>().rect.size));
        width = Mathf.Max(UIConstants.InputNumWidth, textWidth);

        rectTransform.sizeDelta = new Vector2(width, UIConstants.TextFontSize);

        caret.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, textWidth, UIConstants.InputCaretSize);
    }

    public override float PreferredWidth()
    {
        return width;
    }
}