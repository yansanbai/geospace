using System;
using UnityEngine;

public abstract class Operation
{
    public bool CanRotateCamera;
    public bool CanActiveElement;
    public abstract void Start();
    public abstract void End();

    public virtual void OnClickElement(GeoElement element){}
}