using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FunctionCondition : Condition
{
    public int index;
}

public abstract class FunctionConditionTool : ConditionTool
{

}

public class FunctionConstructor : Constructor
{
    private FreeCondition freeCondition;
    new Function geometry;
    private bool geometrySetted;


    public FunctionConstructor(Geometry geometry) : base(geometry)
    {
        if (geometry is Function)
            this.geometry = (Function)geometry;

        geometrySetted = false;
    }

    public override bool AddCondition(Condition condition)
    {
        if (!(condition is FunctionCondition) || geometrySetted)
            return false;

        if (CheckAddCondition((FunctionCondition)condition))
        {
            //Resolve();
            return true;
        }

        return false;
    }

    public override bool RemoveCondition(Condition condition)
    {
        if (!(condition is FreeCondition))
            return false;
        if (condition is FreeCondition)
        {
            FreeCondition free = (FreeCondition)condition;
            geometry.RemoveGeoLine(geometry.Getline()[free.index-1]);
            freeCondition = null;
            return true;
        }
        
        return false;
    }

    public override void ClearConditions()
    {


    }

    private bool CheckAddCondition(FunctionCondition condition)
    {

        if (condition is FunctionCondition)
        {
            if (condition is FreeCondition)
            {
                freeCondition = (FreeCondition)condition;
                return true;
            }

        }

        return true;
    }

    private void Resolve()
    {
       
    }

}
