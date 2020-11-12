using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IElementBehaviour
{
    void SetColorIndex(int index);
    int GetColorIndex();
    Color DefaultColor();
}
