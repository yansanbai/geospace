using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Constructor
{
    protected Geometry geometry;

    public Constructor(Geometry geometry)
    {
        this.geometry = geometry;
    }
    public abstract bool AddCondition(Condition condition);

    public abstract bool RemoveCondition(Condition condition);

    public abstract void ClearConditions();

}