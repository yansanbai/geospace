using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public TestController test;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Show() {
        test.ShowQuestionPanel();
    }
    public void Shut()
    {
        test.ShutQuestionPanel();
    }

    public void Packup() { 

    }
    public void Add() {
        test.AddQuestion();
    }
    public void Back()
    {
        test.BackQuestion();
    }
}
