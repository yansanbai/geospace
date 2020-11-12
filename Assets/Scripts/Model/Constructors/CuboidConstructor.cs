using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CuboidCondition : Condition
{

}

public abstract class CuboidConditionTool : ConditionTool
{

}

public class CuboidConstructor : Constructor
{
    // private List<CuboidCondition> conditions;
    private CuboidLengthCondition lengthCondition;
    private CuboidWidthCondition widthCondition;
    private CuboidHeightCondition heightCondition;

    new Cuboid geometry;

    public CuboidConstructor(Geometry geometry) : base(geometry)
    {
        if (geometry is Cuboid)
            this.geometry = (Cuboid)geometry;
    }

    public override bool AddCondition(Condition condition)
    {
        if (!(condition is CuboidCondition))
            return false;

        if (CheckAddCondition((CuboidCondition)condition))
        {
            Resolve();
            return true;
        }

        return false;

    }

    public override bool RemoveCondition(Condition condition)
    {
        if (!(condition is CuboidCondition))
            return false;
        if (condition is CuboidLengthCondition)
        {
            if (lengthCondition != (CuboidLengthCondition)condition)
                return false;
            lengthCondition = null;
            return true;
        }

        if (condition is CuboidWidthCondition)
        {
            if (widthCondition != (CuboidWidthCondition)condition)
                return false;
            widthCondition = null;
            return true;
        }

        if (condition is CuboidHeightCondition)
        {
            if (heightCondition != (CuboidHeightCondition)condition)
                return false;
            heightCondition = null;
            return true;
        }

        return false;
    }

    public override void ClearConditions()
    {
        lengthCondition = null;
        widthCondition = null;
        heightCondition = null;
    }

    private bool CheckAddCondition(CuboidCondition condition)
    {

        if (condition is CuboidLengthCondition)
        {
            if (lengthCondition != null)
                return false;
            lengthCondition = (CuboidLengthCondition)condition;
            return true;
        }

        if (condition is CuboidWidthCondition)
        {
            if (widthCondition != null)
                return false;
            widthCondition = (CuboidWidthCondition)condition;
            return true;
        }

        if (condition is CuboidHeightCondition)
        {
            if (heightCondition != null)
                return false;
            heightCondition = (CuboidHeightCondition)condition;
            return true;
        }

        return false;
    }

    private void Resolve()
    {
        float length, width, height;
        CuboidLengthWidthHeight(out length, out width, out height);

        if (lengthCondition != null)
            length = lengthCondition.length;

        if (widthCondition != null)
            width = widthCondition.width;

        if (heightCondition != null)
            height = heightCondition.height;

        SetVertices(length, width, height);
    }

    private void SetVertices(float length, float width, float height)
    {
        Vector3 position = new Vector3(width / 2, height / 2, length / 2);
        geometry.SetVerticesAbsPosition(position);
    }

    private void CuboidLengthWidthHeight(out float length, out float width, out float height)
    {
        Vector3 position = geometry.UnitVector(Cuboid.PPP_E);

        length = position.z * 2;
        width = position.x * 2;
        height = position.y * 2;
    }

}
