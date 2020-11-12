using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class Inputable : InputBase, IPointerClickHandler
{
    public Func<bool> OnClickToFocus;
    public abstract void SetFocus(bool focus);

    public abstract void OnPointerClick(PointerEventData data);
}