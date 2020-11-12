using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputText : InputBase
{
    string text = "";
    Text textField;

    public void Init(FormText formText)
    {
        Init();
        Refresh(formText);
    }

    public void Init()
    {
        textField = GetComponent<Text>();

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(UIConstants.TextFontSize * text.Length, UIConstants.TextFontSize);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);

        textField.fontSize = (int)UIConstants.TextFontSize;
    }

    public void Refresh(FormText formText)
    {
        text = formText.label;
        textField.text = text;
    }

    public override float PreferredWidth()
    {
        Font font = Resources.Load<Font>(UIConstants.TextFont);

        int fontsize = (int)UIConstants.TextFontSize;
        font.RequestCharactersInTexture(text, fontsize, FontStyle.Normal);
        CharacterInfo characterInfo;
        float width = 0f;
        for (int i = 0; i < text.Length; i++)
        {
            font.GetCharacterInfo(text[i], out characterInfo, fontsize);
            width += characterInfo.advance;
        }

        return width;
        // return textField.cachedTextGenerator.GetPreferredWidth(text, textField.GetGenerationSettings(GetComponent<RectTransform>().rect.size));
    }

    public void SetTimer()
    {
        // Destroy(gameObject, 1.5f);
    }
}