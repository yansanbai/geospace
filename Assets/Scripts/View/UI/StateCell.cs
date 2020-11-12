using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum StateCellState
{
    Open,
    Close,
}

public class StateCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnClickDelete;

    public Action UndoFaceHighlight;

    public Action DoubleClick;

    // StateCellState state;

    public Action OnElementHighlight;  //ElementPanel的OnElementClickColor

    RectTransform rectTransform;
    GameObject btnToggleObject;
    GameObject btnDeleteObject;
    //DoubleClickButton btnToggle;
    Button btnToggle;
    Button btnDelete;
    Text text;
    Boolean isHighlighted = false;

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();

        btnToggleObject = transform.Find("ButtonToggle").gameObject;
        btnDeleteObject = transform.Find("ButtonDelete").gameObject;

        //btnToggle = btnToggleObject.GetComponent<DoubleClickButton>();
        btnToggle = btnToggleObject.GetComponent<Button>();
        btnDelete = btnDeleteObject.GetComponent<Button>();
    
        btnToggle.onClick.AddListener(ToggleButtonDoubleClicked);
        btnDelete.onClick.AddListener(DeleteButtonClicked);

        text = transform.Find("Text").GetComponent<Text>();
    }


    public void SetText(string str)
    {
        text.text = str;
    }

    public void SetForm(FormInput form)
    {
        string str = "";
        foreach (FormItem item in form.inputs)
            str += item.ToString() + " ";

        text.text = str;

    }

    public void SetIcon(Sprite sprite)
    {
        Image image = btnToggle.transform.Find("Icon").GetComponent<Image>();
        image.sprite = sprite;
    }

    public void SetTintColor(Color color)
    {
        Image image = btnToggle.GetComponent<Image>();
        image.color = color;
    }

    public void SetPosY(float y)
    {
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, rectTransform.rect.height);
    }

    public float GetHeight()
    {
        // if (state == StateCellState.Open)
        //     return UIConstants.ToolCloseButtonWidth + ButtonsWidth;

        return UIConstants.StateCellHeight;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        btnDeleteObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data)
    {
        btnDeleteObject.SetActive(false);
    }

    void DeleteButtonClicked()
    {
        if(UndoFaceHighlight != null&&isHighlighted == true)
            UndoFaceHighlight();
        if (OnClickDelete != null)
            OnClickDelete(); 
    }

    void ToggleButtonDoubleClicked()
    {
        
        if(isHighlighted == true)
        {
            if(UndoFaceHighlight != null)
            UndoFaceHighlight();
            isHighlighted = false;
        }
        else
        {
            if (OnElementHighlight != null)
            {
                OnElementHighlight();
            }
            if(DoubleClick != null)
            {
                DoubleClick();
            }
            isHighlighted = true;
        }
        
    }

        void SetState(StateCellState s)
    {
        // state = s;
        // btnWrap.SetActive(state == ButtonGroupState.Open);
        // btnOpenObject.SetActive(state == ButtonGroupState.Close);
        // btnCloseObject.SetActive(state == ButtonGroupState.Open);

        // if (OnStateChange != null)
        //     OnStateChange();

    }

}

