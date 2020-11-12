using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElementBehaviour : MonoBehaviour
{
    protected int colorIndex;
    protected int styleIndex;
    protected bool visiable;

    public virtual void SetColorIndex(int index)
    {
        colorIndex = index;
    }

    public virtual void SetStyleIndex(int index)
    {
        styleIndex = index;
    }
    
    public virtual void SetVisible(bool v)
    {
        visiable = v;
        gameObject.SetActive(visiable);
    }

    public virtual bool GetVisible()
    {
        return visiable;
    }

}
