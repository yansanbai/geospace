using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum InputPanelState
{
    Normal,
    Message,
    Highlight,
    Error,
    Valid,
}
public class InputPanel : MonoBehaviour
{
    public InputElement InputElementPrefab;
    public InputNum InputNumPrefab;
    public InputText InputTextPrefab;

    public Action<FormInput> OnClickSubmit;
    public Action<FormInput> OnClickCancel;
    public Action<FormInput> OnInputChanged;
    public Func<FormInput, bool> OnValidate;

    InputPanelState state;
    Image frame;
    Button btnSubmit;
    Button btnCancel;
    InputBase[] inputBases;
    List<Inputable> inputables;
    Inputable focusInputable;

    FormInput form;

    public void Init()
    {
        frame = transform.Find("Frame").GetComponent<Image>();

        btnSubmit = transform.Find("ButtonSubmit").GetComponent<Button>();
        btnCancel = transform.Find("ButtonCancel").GetComponent<Button>();

        SetState(InputPanelState.Normal);

        SetTintColor();
        StyleManager.OnStyleChange += () =>
        {
            SetTintColor();
        };

        KeyboardSystem.OnKey.AddListener(InputKey);
        btnSubmit.onClick.AddListener(ClickSubmit);
        btnCancel.onClick.AddListener(ClickCancel);
    }

    public void OnDestroy()
    {
        if (KeyboardSystem.instance)
            KeyboardSystem.OnKey.RemoveListener(InputKey);
    }

    public void SetState(InputPanelState s)
    {
        state = s;
        bool isShowButtons = state != InputPanelState.Normal && state != InputPanelState.Message;
        btnSubmit.gameObject.SetActive(isShowButtons);
        btnCancel.gameObject.SetActive(isShowButtons);
        btnSubmit.interactable = state == InputPanelState.Valid;
        SetTintColor();
    }

    public InputPanelState GetState()
    {
        return state;
    }

    private void InputKey(KeyCode keycode)
    {
        if (state == InputPanelState.Normal)
            return;
        if (keycode == KeyCode.Tab)
            NextFoucs();
        if (keycode == KeyCode.Return)
            ClickSubmit();
    }

    public void Clear()
    {
        form = null;

        if (inputables != null)
        {
            inputables.Clear();
            inputables = null;
        }

        if (inputBases != null)
        {
            for (int i = 0; i < inputBases.Length; i++)
            {
                if(inputBases[i]!=null && inputBases[i].gameObject!=null){
                    Destroy(inputBases[i].gameObject);
                }
            }

            inputBases = null;
        }

        OnInputChanged = null;
        OnValidate = null;
        OnClickSubmit = null;
        OnClickCancel = null;

        SetState(InputPanelState.Normal);
    }

    public void SetFormForMessage(FormInput formInput)
    {
        SetForm(formInput);

        SetState(InputPanelState.Message);
    }
    public void SetFormForInput(FormInput formInput)
    {
        Clear();
        SetForm(formInput);

        SetState(InputPanelState.Highlight);

        foreach (Inputable inputable in inputables)
        {
            inputable.OnClickToFocus += () =>
            {
                if (focusInputable == inputable)
                    return false;
                SetFocus(inputable);
                return true;
            };
        }

        NextFoucs();
    }

    private void SetForm(FormInput formInput)
    {
        if (state != InputPanelState.Normal)
            return;

        form = formInput;
        FormItem[] inputs = form.inputs;
        inputBases = new InputBase[inputs.Length];

        inputables = new List<Inputable>();
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] is FormElement)
            {
                FormElement formElement = (FormElement)inputs[i];
                InputElement inputElement = AddInputElement(formElement);
                inputables.Add(inputElement);
                inputBases[i] = inputElement;
            }
            else if (inputs[i] is FormNum)
            {
                FormNum formNum = (FormNum)inputs[i];
                InputNum inputNum = AddInputNum(formNum);
                inputables.Add(inputNum);
                inputBases[i] = inputNum;
            }
            else if (inputs[i] is FormText)
            {
                FormText formText = (FormText)inputs[i];
                InputText inputText = AddInputText(formText);
                inputBases[i] = inputText;
            }
        }

        RefreshLayout();
    }

    public void RefreshForm(FormInput formInput)
    {
        if (formInput != form)
            return;

        FormItem[] inputs = form.inputs;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] is FormElement)
            {
                FormElement formElement = (FormElement)inputs[i];
                InputElement inputElement = (InputElement)inputBases[i];
                inputElement.Refresh(formElement);
            }
            else if (inputs[i] is FormNum)
            {
                FormNum formNum = (FormNum)inputs[i];
                InputNum inputNum = (InputNum)inputBases[i];
                inputNum.Refresh(formNum);
            }
            else if (inputs[i] is FormText)
            {
                FormText formText = (FormText)inputs[i];
                InputText inputText = (InputText)inputBases[i];
                inputText.Refresh(formText);
            }
        }
    }

    public void InputFields(string[] fields)
    {
        if (!(focusInputable is InputElement))
            return;

        InputElement inputElement = (InputElement)focusInputable;
        int rest = inputElement.RestLength();
        if (fields.Length > rest)
            return;
        foreach (string sign in fields)
        {
            inputElement.InputVertex(sign);
        }
        if (fields.Length == rest)
            NextFoucs();
    }

    private void SetFocus(Inputable inputable)
    {
        if (focusInputable != null)
            focusInputable.SetFocus(false);
        inputable.SetFocus(true);
        focusInputable = inputable;
    }

    private void NextFoucs()
    {
        if (inputables == null || inputables.Count == 0)
            return;

        if (focusInputable == null)
        {
            SetFocus(inputables[0]);
        }
        else if (focusInputable != inputables[inputables.Count - 1])
        {
            int index = inputables.IndexOf(focusInputable);
            SetFocus(inputables[index + 1]);
        }
    }

    private void ClickSubmit()
    {
        if (state != InputPanelState.Valid)
            return;

        if (OnClickSubmit != null)
            OnClickSubmit(form);
    }

    public void ClickCancel()
    {
        if (state == InputPanelState.Normal)
            return;

        if (OnClickCancel != null)
            OnClickCancel(form);
    }

    private void Validate()
    {
        if (state == InputPanelState.Message)
            return;
        bool result = true;
        if (OnValidate != null)
            result = OnValidate(form);
        SetState(result ? InputPanelState.Valid : InputPanelState.Error);
    }

    private InputElement AddInputElement(FormElement formElement)
    {
        GameObject go = GameObject.Instantiate(InputElementPrefab.gameObject);
        go.transform.SetParent(transform, false);

        InputElement inputElement = go.GetComponent<InputElement>();
        inputElement.Init(formElement);

        inputElement.OnInputChanged += (strings) =>
        {
            formElement.fields = strings;
            if (OnInputChanged != null)
                OnInputChanged(form);
            RefreshLayout();
            Validate();
        };
        return inputElement;
    }

    private InputNum AddInputNum(FormNum formNum)
    {
        Debug.Log("input");
        GameObject go = GameObject.Instantiate(InputNumPrefab.gameObject);
        go.transform.SetParent(transform, false);

        InputNum inputNum = go.GetComponent<InputNum>();
        inputNum.Init(formNum);

        inputNum.OnInputChanged += (num) =>
        {
            formNum.num = num;
            if (OnInputChanged != null)
                OnInputChanged(form);
            RefreshLayout();
            Validate();
        };
        return inputNum;
    }

    private InputText AddInputText(FormText formText)
    {
        GameObject go = GameObject.Instantiate(InputTextPrefab.gameObject);
        go.transform.SetParent(transform, false);

        InputText inputText = go.GetComponent<InputText>();
        inputText.Init(formText);
        return inputText;
    }

    private void RefreshLayout()
    {
        float posX = UIConstants.InputSpacing * 2;

        foreach (InputBase inputBase in inputBases)
        {
            SetPosition(inputBase, posX);
            posX += inputBase.PreferredWidth() + UIConstants.InputSpacing;
        }

    }

    private void SetPosition(InputBase inputBase, float posX)
    {
        RectTransform rt = inputBase.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, rt.sizeDelta.x);
    }

    private void SetTintColor()
    {
        switch (state)
        {
            case InputPanelState.Highlight:
                frame.color = StyleManager.Highlight;
                break;
            case InputPanelState.Error:
                frame.color = StyleManager.Error;
                break;
            case InputPanelState.Valid:
                frame.color = StyleManager.Valid;
                break;
            default:
                frame.color = Color.clear;
                break;
        }
    }
    public void SetTimerMessage(FormInput formInput)
    {
        SetFormForMessage(formInput);

        for (int i = 0; i < inputBases.Length; i++) {
            InputBase inputBase = inputBases[i];
            if (inputBase is InputText)
            {
                InputText inputText = (InputText) inputBase;
                inputText.SetTimer();
            }
        }
    }
}
