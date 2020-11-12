using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputElement : Inputable
{
    const int MAX_COUNT = 5;
    public InputUnit InputPrefab;

    public Action<string[]> OnInputChanged;

    float unitSize;

    bool canExpand;
    int baseCount;
    int count;
    bool isFocus;
    List<InputUnit> inputs;
    List<string> strings;
    RectTransform caret;
    int activeIndex;

    public void Init(FormElement formElement)
    {
        Init(formElement.signCount);

        Refresh(formElement);
    }
    public void Init(int c)
    {
        canExpand = c < 0;
        c = Mathf.Abs(c);
        baseCount = c;

        KeyboardSystem.OnKey.AddListener(InputKey);

        unitSize = UIConstants.TextFontSize + UIConstants.TextFontSize * UIConstants.TextFontSubRatio / 2;

        GameObject caretObject = transform.Find("Caret").gameObject;
        caret = caretObject.GetComponent<RectTransform>();
        caret.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);

        inputs = new List<InputUnit>(baseCount);
        strings = new List<string>(baseCount);

        for (int i = 0; i < baseCount; i++)
            AddInputUnit();

        RefreshRect();

        SetFocus(false);
        SetActiveIndex(0);
    }

    public void Refresh(FormElement formElement)
    {
        string[] strs = formElement.fields;
        int startIndex = 0;
        for (int i = 0; i < Mathf.Min(formElement.count, strs.Length); i++)
        {
            if (inputs[i].SetString(strs[i]))
            {
                strings[i] = strs[i];
                startIndex++;
            }
            else
                break;
        }

        SetActiveIndex(startIndex);
        TextChanged();
    }

    private void AddInputUnit()
    {
        GameObject unitObject = GameObject.Instantiate(InputPrefab.gameObject);
        unitObject.transform.SetParent(transform, false);
        InputUnit inputUnit = unitObject.GetComponent<InputUnit>();
        inputUnit.Init();

        RectTransform unitRT = unitObject.GetComponent<RectTransform>();

        float posX = unitSize * count;
        unitRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, unitSize);
        inputs.Add(inputUnit);

        count++;
        strings.Add("");
    }

    private void RemoveInputUnit()
    {
        InputUnit inputUnit = inputs[count - 1];

        inputs.RemoveAt(count - 1);
        strings.RemoveAt(count - 1);
        Destroy(inputUnit.gameObject);

        count--;
    }

    private void RefreshRect()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(unitSize * count, UIConstants.TextFontSize);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);
    }

    private bool Expand()
    {
        if (!canExpand)
            return false;

        if (activeIndex == count && count < MAX_COUNT)
        {
            AddInputUnit();
            RefreshRect();
            return true;
        }

        return false;
    }

    private bool Reduce()
    {
        if (!canExpand)
            return false;
        if (activeIndex < count && activeIndex >= baseCount)
        {
            RemoveInputUnit();
            RefreshRect();
            return true;
        }

        return false;
    }

    public void OnDestroy()
    {
        if (KeyboardSystem.instance)
            KeyboardSystem.OnKey.RemoveListener(InputKey);
    }

    public int Count()
    {
        return count;
    }


    public int Length()
    {
        return activeIndex;
    }

    public int RestLength()
    {
        if (canExpand)
            return MAX_COUNT - activeIndex;
        return count - activeIndex;
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

    public void InputVertex(string sign)
    {
        if (!isFocus)
            return;

        InputChanged(InputSign(sign));
    }

    private void InputKey(KeyCode keycode)
    {
        // Debug.Log("Key held down: " + keycode);
        if (!isFocus)
            return;
        if (keycode >= KeyCode.A && keycode <= KeyCode.Z)
        {
            char c = (char)(keycode - KeyCode.A + 'A');
            InputChanged(InputLetter(c));
        }
        else if (keycode >= KeyCode.Alpha1 && keycode <= KeyCode.Alpha9)
        {
            char c = (char)(keycode - KeyCode.Alpha0 + '0');
            InputChanged(InputNum(c));
        }
        else if (keycode == KeyCode.Delete || keycode == KeyCode.Backspace)
            InputChanged(InputDelete());
    }

    private void InputChanged(int index)
    {
        if (index >= 0 && index < count)
            strings[index] = inputs[index].GetText();
        // Debug.Log(index + "   changed! " + string.Join(" ", strings.ToArray()));
        TextChanged();
    }

    private void TextChanged()
    {
        if (OnInputChanged != null)
            OnInputChanged(strings.ToArray());
    }

    private int InputSign(string sign)
    {
        if (activeIndex == count)
            if (!Expand())
                return -1;

        if (sign.Length > 2)
            return -1;

        bool result = inputs[activeIndex].SetString(sign);
        if (result)
        {
            int changedIndex = activeIndex;
            SetActiveIndex(activeIndex + 1);
            return changedIndex;
        }
        return -1;
    }

    private int InputLetter(char letter)
    {
        if (activeIndex == count)
            if (!Expand())
                return -1;
        bool result = inputs[activeIndex].AddChar(letter);
        if (result)
        {
            int changedIndex = activeIndex;
            SetActiveIndex(activeIndex + 1);
            return changedIndex;
        }
        return -1;
    }

    private int InputNum(char num)
    {
        if (activeIndex == 0)
            return -1;
        bool result = inputs[activeIndex - 1].AddChar(num);
        if (result)
        {
            return activeIndex - 1;
        }
        return -1;

    }
    private int InputDelete()
    {
        if (activeIndex == 0)
            return -1;
        bool result = inputs[activeIndex - 1].DeleteChar();
        if (result)
        {
            bool empty = inputs[activeIndex - 1].IsEmpty();
            int changedIndex = activeIndex - 1;
            if (empty)
            {
                SetActiveIndex(activeIndex - 1);
                Reduce();
            }
            return changedIndex;
        }
        return -1;
    }

    private void SetActiveIndex(int index)
    {
        if (index < 0 || index > count)
            return;
        activeIndex = index;

        float posX = unitSize * index;
        float size = UIConstants.InputCaretSize;
        caret.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, size);
    }

    public override float PreferredWidth()
    {
        return unitSize * count;
    }
}