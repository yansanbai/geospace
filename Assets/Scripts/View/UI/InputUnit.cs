using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Text))]
public class InputUnit : MonoBehaviour
{
    private string text;
    private Text textField;

    private int subSize;

    public void Init()
    {
        textField = GetComponent<Text>();
        textField.fontSize = (int)UIConstants.TextFontSize;
        subSize = UIConstants.TextFontSubSize;
        SetText("");
    }

    public string GetText()
    {
        return text;
    }

    private void SetText(string str)
    {
        text = str;
        textField.text = UIConstants.SignFormat(text, subSize);
    }

    public void Clear()
    {
        SetText("");
    }

    public bool IsEmpty()
    {
        return text == "";
    }

    public bool SetString(string str)
    {
        if (str == null || str == "" || str.Length > 2)
            return false;

        if (str.Length == 2)
        {
            char c1 = str[1];
            bool can1 = IsNumChar(c1, out c1);
            if (!can1)
                return false;
        }

        char c0 = str[0];
        bool canSet = IsLetterChar(c0, out c0);
        if (canSet)
        {
            str = c0 + str.Substring(1);
            SetText(str);
            return true;
        }
        return false;
    }

    public bool AddChar(char addedChar)
    {
        if (text.Length >= 2)
            return false;

        if (text.Length == 0)
        {
            char outChar;
            bool canAdd = IsLetterChar(addedChar, out outChar);
            if (canAdd)
                SetText(outChar + "");
            return canAdd;
        }

        if (text.Length == 1)
        {
            char outChar;
            bool canAdd = IsNumChar(addedChar, out outChar);
            if (canAdd)
                SetText(text + outChar);
            return canAdd;
        }

        return false;
    }

    public bool DeleteChar()
    {
        if (text.Length == 0)
            return false;
        SetText(text.Substring(0, text.Length - 1));
        return true;
    }

    private bool IsLetterChar(char inChar, out char outChar)
    {
        if (inChar >= 'A' && inChar <= 'Z')
        {
            outChar = inChar;
            return true;
        }
        if (inChar >= 'a' && inChar <= 'z')
        {
            outChar = (char)(inChar - 'a' + 'A');
            return true;
        }

        outChar = '\0';
        return false;
    }

    private bool IsNumChar(char inChar, out char outChar)
    {
        if (inChar >= '0' && inChar <= '9')
        {
            outChar = inChar;
            return true;
        }

        outChar = '\0';
        return false;
    }
}