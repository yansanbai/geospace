using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FormItem
{

}

public class FormText : FormItem
{
    public string label;

    public FormText(string l)
    {
        label = l;
    }

    public override string ToString()
    {
        return label;
    }
}

public class FormElement : FormItem
{
    public int count;
    public int signCount;
    public string[] fields;

    public FormElement(int c)
    {
        signCount = c;
        count = Mathf.Abs(c);
        fields = new string[count];
    }

    public FormElement(int c, string[] strs)
    {
        signCount = c;
        count = Mathf.Abs(c);
        fields = new string[count];
        for (int i = 0; i < count; i++)
        {
            fields[i] = strs[i];
        }
    }

    public bool IsFull()
    {
        for (int i = 0; i < count; i++)
        {
            if (fields[i] == null || fields[i] == "")
                return false;
        }
        return true;
    }

    public bool IsEmpty()
    {
         for (int i = 0; i < count; i++)
        {
            if (fields[i] != null && fields[i] != "")
                return false;
        }
        return true;
    }

    public int CurrentLength()
    {
        int len = 0;
        for (int i = 0; i < count; i++)
        {
            if (fields[i] == null || fields[i] == "")
                return len;
            len++;
        }
        return len;
    }

    public string ToString(int subSize)
    {
        string str = "";
        foreach (string sign in fields)
        {
            str += UIConstants.SignFormat(sign, subSize);
        }
        return str;
    }

    public override string ToString()
    {
        return ToString(UIConstants.TextFontSubSize);
    }
}

public class FormNum : FormItem
{
    public float num;
    public bool isEmpty;
    public string format = "";

    public FormNum()
    {
        isEmpty = true;
    }

    public FormNum(float n)
    {
        isEmpty = false;
        num = n;
    }

    public override string ToString()
    {
        return num.ToString(format);
    }
}


public class FormInput
{
    public FormItem[] inputs;

    public FormInput(int c)
    {
        inputs = new FormItem[c];
    }
}