using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class State
{
    public Tool tool;

    public Action OnClickDelete;

    public Action OnElementHighlight; //Requirement 1

    public Action DoubleClick; //Requirement 1

    public Action UndoFaceHighlight; //Requirement 1


    public State(Tool tool)
    {
        this.tool = tool;
    }

    public abstract int[] DependVertices();
    public abstract FormInput Title();
}
