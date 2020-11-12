using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecognizePanel : MonoBehaviour
{
    InputField input;
    InputPanel inputPanel;

    public void Init()
    {
        input = transform.Find("InputField").GetComponent<InputField>();
        inputPanel = GameObject.Find("/UI/CanvasBack").transform.Find("InputPanel").GetComponent<InputPanel>();
        Clear();
    }

    public void AddWord(string str)
    {
        input.text += str;
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        input.text = "";
    }

    public void showRecognizePanel()
    {
        Debug.Log("happen");
        gameObject.SetActive(true);
        inputPanel.Clear();
    }

    public string GetWords()
    {
        return input.text;
    }

    public void OnDestroy()
    {
    }
}
