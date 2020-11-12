using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class TriPyramidCondition : Condition
{
    public int order;

    public TriPyramidCondition(int o)
    {
        order = o;
    }
}

public abstract class TriPyramidConditionTool : ConditionTool
{
    protected bool IsTopEdge(TriPyramid triPyramid, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 2)
            return false;

        string[] fields = formElement.fields;

        bool result = (triPyramid.IsSignTop(fields[0]) && triPyramid.IsSignBottom(fields[1])) ||
            (triPyramid.IsSignTop(fields[1]) && triPyramid.IsSignBottom(fields[0]));

        return result;
    }

    protected bool IsBottomEdge(TriPyramid triPyramid, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 2)
            return false;

        string[] fields = formElement.fields;

        bool result = triPyramid.IsSignBottom(fields[0]) && triPyramid.IsSignBottom(fields[1]);
        result = result && (fields[0] != fields[1]);

        return result;
    }

    protected bool IsBottomCorner(TriPyramid triPyramid, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 3)
            return false;

        string[] fields = formElement.fields;

        bool result = triPyramid.IsSignBottom(fields[0]) && triPyramid.IsSignBottom(fields[1]) && triPyramid.IsSignBottom(fields[2]);
        result = result && (fields[0] != fields[1]) && (fields[0] != fields[2]) && (fields[1] != fields[2]);

        return result;
    }

    protected bool IsTopTCorner(TriPyramid triPyramid, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 3)
            return false;

        string[] fields = formElement.fields;

        bool result = triPyramid.IsSignTop(fields[1]) && triPyramid.IsSignBottom(fields[0]) && triPyramid.IsSignBottom(fields[2]);
        result = result && (fields[0] != fields[1]) && (fields[0] != fields[2]) && (fields[1] != fields[2]);

        return result;
    }

    protected bool IsTopBCorner(TriPyramid triPyramid, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 3)
            return false;

        string[] fields = formElement.fields;

        bool result = (triPyramid.IsSignTop(fields[0]) && triPyramid.IsSignBottom(fields[1]) && triPyramid.IsSignBottom(fields[2])) ||
            (triPyramid.IsSignTop(fields[2]) && triPyramid.IsSignBottom(fields[1]) && triPyramid.IsSignBottom(fields[0]));
        result = result && (fields[0] != fields[1]) && (fields[0] != fields[2]) && (fields[1] != fields[2]);

        return result;
    }

    protected FormElement EdgesToCorner(FormElement formElement1, FormElement formElement2)
    {
        FormElement result = new FormElement(3);
        if (!formElement1.IsFull() || !formElement2.IsFull())
            return result;
        if (formElement1.count != 2 || formElement2.count != 2)
            return result;

        string[] fields1 = formElement1.fields;
        string[] fields2 = formElement2.fields;

        Dictionary<string, int> map = new Dictionary<string, int>();
        MapIncrement(map, fields1[0]);
        MapIncrement(map, fields1[1]);
        MapIncrement(map, fields2[0]);
        MapIncrement(map, fields2[1]);

        List<KeyValuePair<string, int>> list = map.OrderBy(o => o.Value).ToList();

        if (list.Count != 3)
            return result;

        result.fields[0] = list[0].Key;
        result.fields[1] = list[2].Key;
        result.fields[2] = list[1].Key;

        return result;
    }

    private void MapIncrement(Dictionary<string, int> map, string key)
    {
        if (map.ContainsKey(key))
            map[key]++;
        else
            map.Add(key, 0);
    }
}

public abstract class TriPyramidBottomCondition : TriPyramidCondition
{
    public TriPyramidBottomCondition(int o) : base(o)
    { }
}

public abstract class TriPyramidTopCondition : TriPyramidCondition
{
    public TriPyramidTopCondition(int o) : base(o)
    { }
}

public class TriPyramidConstructor : Constructor
{
    private TriPyramidBottomConstructor bottomConstructor;
    private TriPyramidTopConstructor topConstructor;

    public TriPyramidConstructor(Geometry geometry) : base(geometry)
    {
        this.geometry = geometry;
        bottomConstructor = new TriPyramidBottomConstructor(geometry);
        topConstructor = new TriPyramidTopConstructor(geometry);
    }

    public override bool AddCondition(Condition condition)
    {
        if (!(condition is TriPyramidCondition))
            return false;

        if (condition is TriPyramidBottomCondition)
            return AddBottomCondition(condition);

        else if (condition is TriPyramidTopCondition)
            return AddTopCondition(condition);

        return false;
    }

    public override bool RemoveCondition(Condition condition)
    {
        if (!(condition is TriPyramidCondition))
            return false;

        if (condition is TriPyramidBottomCondition)
            return bottomConstructor.RemoveCondition(condition);

        else if (condition is TriPyramidTopCondition)
            return topConstructor.RemoveCondition(condition);

        return false;
    }

    public override void ClearConditions()
    {
        bottomConstructor.ClearConditions();
        topConstructor.ClearConditions();
    }
    
    private bool AddBottomCondition(Condition condition)
    {
        bottomConstructor.SetCache();

        bottomConstructor.AddCondition(condition);
        bool resolve = bottomConstructor.Resolve();

        if (!resolve)
        {
            bottomConstructor.RemoveCondition(condition);
            return false;
        }

        resolve = topConstructor.Resolve();

        if (!resolve)
        {
            bottomConstructor.RemoveCondition(condition);
            bottomConstructor.CacheCallBack();
            return false;
        }

        return resolve;
    }

    private bool AddTopCondition(Condition condition)
    {
        topConstructor.AddCondition(condition);
        bool resolve = topConstructor.Resolve();

        if (!resolve)
        {
            bottomConstructor.RemoveCondition(condition);
            return false;
        }

        return resolve;
    }

}