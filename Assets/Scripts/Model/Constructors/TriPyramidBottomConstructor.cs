using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TriPyramidBottomConstructor : Constructor
{
    private List<TriPyramidBottomCondition> bottomConditions;

    public TriPyramidBottomConstructor(Geometry geometry) : base(geometry)
    {
        this.geometry = geometry;
        bottomConditions = new List<TriPyramidBottomCondition>();
    }

    public override bool AddCondition(Condition condition)
    {
        if (condition is TriPyramidBottomCondition)
        {
            TriPyramidBottomCondition addCondition = (TriPyramidBottomCondition)condition;

            bool result = CheckAddCondition(addCondition);

            // if (!result)
            // {
            //     Debug.LogWarning("Error In Try");
            //     return false;
            // }

            bottomConditions.Add(addCondition);
            bottomConditions.Sort(ConditionComparer);

            // result = ResolveBottom();

            // if (!result)
            // {
            //     bottomConditions.Remove(addCondition);
            //     return false;
            // }

            return result;
        }

        return false;
    }

    public override bool RemoveCondition(Condition condition)
    {
        if (condition is TriPyramidBottomCondition)
            return bottomConditions.Remove((TriPyramidBottomCondition)condition);

        return false;
    }

    public override void ClearConditions()
    {
        bottomConditions.Clear();
    }

    public struct BottomCache
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
    }

    BottomCache bottomCache;

    public void SetCache()
    {
        bottomCache = new BottomCache();
        bottomCache.pointA = geometry.UnitVector(TriPyramid.BOTTOM_A);
        bottomCache.pointB = geometry.UnitVector(TriPyramid.BOTTOM_B);
        bottomCache.pointC = geometry.UnitVector(TriPyramid.BOTTOM_C);
    }

    public void CacheCallBack()
    {
        geometry.SetUnitVector(TriPyramid.BOTTOM_A, bottomCache.pointA);
        geometry.SetUnitVector(TriPyramid.BOTTOM_B, bottomCache.pointB);
        geometry.SetUnitVector(TriPyramid.BOTTOM_C, bottomCache.pointC);

    }

    public bool Resolve()
    {
        if (bottomConditions.Count == 0)
            return true;

        BottomConverter converter;
        if (ResolveBottom(out converter))
            return SetBottom(converter);
        return false;
    }

    private bool CheckAddCondition(TriPyramidBottomCondition condition)
    {
        // if (bottomConditions.Count == 3)
        //     return false;

        // // check conflictions
        // foreach (TriPyramidBottomCondition c in bottomConditions)
        // {
        //     if (c is BottomLengthCondition && condition is BottomLengthCondition)
        //     {
        //         if (((BottomLengthCondition)c).edge == ((BottomLengthCondition)condition).edge)
        //             return false;
        //     }
        //     else if (c is BottomAngleCondition && condition is BottomAngleCondition)
        //     {
        //         if (((BottomAngleCondition)c).corner == ((BottomAngleCondition)condition).corner)
        //             return false;
        //     }
        // }

        // // special
        // if (bottomConditions.Count == 2)
        // {
        //     if (bottomConditions[0] is BottomAngleCondition && bottomConditions[1] is BottomAngleCondition && condition is BottomAngleCondition)
        //         return false;
        // }

        return true;
    }

    private int ConditionComparer(TriPyramidBottomCondition c1, TriPyramidBottomCondition c2)
    {
        return c1.order.CompareTo(c2.order);
    }

    private class BottomConverter
    {
        public float a, b, c;
        public EdgeRefer aER, bER, cER;
        public float lenAB, lenAC, lenBC;

        public void Resolve()
        {
            if (aER == new EdgeRefer(0, 1))
                lenAB = a;
            else if (bER == new EdgeRefer(0, 1))
                lenAB = b;
            else if (cER == new EdgeRefer(0, 1))
                lenAB = c;

            if (aER == new EdgeRefer(0, 2))
                lenAC = a;
            else if (bER == new EdgeRefer(0, 2))
                lenAC = b;
            else if (cER == new EdgeRefer(0, 2))
                lenAC = c;

            if (aER == new EdgeRefer(1, 2))
                lenBC = a;
            else if (bER == new EdgeRefer(1, 2))
                lenBC = b;
            else if (cER == new EdgeRefer(1, 2))
                lenBC = c;
        }
    }

    private void ConverterSetABC(BottomConverter converter, float a, float b, float c)
    {
        converter.a = a;
        converter.b = b;
        converter.c = c;
    }

    private delegate bool ResolveBottomDelegate(BottomConverter converter, out bool judge);

    private bool ResolveBottom(out BottomConverter converter)
    {
        converter = new BottomConverter();

        ResolveBottomDelegate[] resolves1 = {
            ResolveBottomA,
            ResolveBottomL,
        };

        ResolveBottomDelegate[] resolves2 = {
            ResolveBottomAA,
            ResolveBottomLL,
            ResolveBottomLA,
            ResolveBottomAL,
         };

        ResolveBottomDelegate[] resolves3 = {
            ResolveBottomLLL,
            ResolveBottomLAL,
            ResolveBottomLLA,
            ResolveBottomALA,
        };

        ResolveBottomDelegate[][] resolvesArray = {
            resolves1,
            resolves2,
            resolves3,
        };

        ResolveBottomDelegate[] resolves = null;
        for (int i = 0; i < resolvesArray.Length; i++)
        {
            if (bottomConditions.Count == i + 1)
            {
                resolves = resolvesArray[i];
                break;
            }
        }

        if (resolves == null)
            return false;

        foreach (ResolveBottomDelegate resolve in resolves)
        {
            bool judge;
            if (resolve(converter, out judge))
            {
                Debug.Log(resolve);
                if (judge)
                {
                    converter.Resolve();
                    return true;
                }
                else
                {
                    Debug.LogWarning("Error Condition In Resolve");
                    return false;
                }
            }
        }

        return false;
    }

    private bool SetBottom(BottomConverter converter)
    {
        if (converter == null)
            return false;
        return SetBottomVertex(converter.lenAB, converter.lenAC, converter.lenBC);
    }

    // A  ->  LAL
    private bool ResolveBottomA(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 1 &&
            bottomConditions[0] is BottomAngleCondition)
        {
            BottomAngleCondition condition = (BottomAngleCondition)bottomConditions[0];

            float a, b, c, alpha;

            converter.aER = new EdgeRefer(condition.corner.id1, condition.corner.id2);
            converter.bER = new EdgeRefer(condition.corner.id3, condition.corner.id2);
            converter.cER = new EdgeRefer(condition.corner.id1, condition.corner.id3);

            a = EdgeIndexLength(condition.corner.id1, condition.corner.id2);
            b = EdgeIndexLength(condition.corner.id3, condition.corner.id2);
            c = EdgeReferLength(converter.cER);
            alpha = condition.angle;

            judge = BottomLAL(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }
        return false;
    }

    // L  ->  LAL
    private bool ResolveBottomL(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 1 &&
            bottomConditions[0] is BottomLengthCondition)
        {
            BottomLengthCondition condition = (BottomLengthCondition)bottomConditions[0];

            float a, b, c, alpha;

            int id3 = EdgeOppositeIndex(condition.edge);

            converter.aER = new EdgeRefer(condition.edge.id1, condition.edge.id2);
            converter.bER = new EdgeRefer(condition.edge.id1, id3);
            converter.cER = new EdgeRefer(condition.edge.id2, id3);

            a = condition.length;
            b = EdgeIndexLength(condition.edge.id1, id3);
            c = EdgeReferLength(converter.cER);
            alpha = CornerIndexAngle(condition.edge.id2, condition.edge.id1, id3);

            judge = BottomLAL(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }
        return false;
    }

    // LL  ->  LAL
    private bool ResolveBottomLL(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 2 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomLengthCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomLengthCondition condition2 = (BottomLengthCondition)bottomConditions[1];

            if (condition1.edge.id1 == condition2.edge.id2)
                condition2.edge.SwapIndex();
            if (condition1.edge.id2 == condition2.edge.id1)
                condition1.edge.SwapIndex();
            if (condition1.edge.id2 == condition2.edge.id2)
            {
                condition1.edge.SwapIndex();
                condition2.edge.SwapIndex();
            }

            float a, b, c, alpha;

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.cER = new EdgeRefer(condition1.edge.id2, condition2.edge.id2);

            a = condition1.length;
            b = condition2.length;
            c = EdgeReferLength(converter.cER);
            alpha = CornerIndexAngle(condition1.edge.id2, condition1.edge.id1, condition2.edge.id2);

            judge = BottomLAL(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }
        return false;
    }

    // AA -> ALA
    private bool ResolveBottomAA(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 2 &&
            bottomConditions[0] is BottomAngleCondition && bottomConditions[1] is BottomAngleCondition)
        {
            BottomAngleCondition condition1 = (BottomAngleCondition)bottomConditions[0];
            BottomAngleCondition condition2 = (BottomAngleCondition)bottomConditions[1];

            if (condition1.corner.id1 == condition2.corner.id3)
                condition2.corner.SwapIndex();
            if (condition1.corner.id3 == condition2.corner.id1)
                condition1.corner.SwapIndex();
            if (condition1.corner.id3 == condition2.corner.id3)
            {
                condition1.corner.SwapIndex();
                condition2.corner.SwapIndex();
            }

            float a, b, c, alpha, beta;

            converter.aER = new EdgeRefer(condition1.corner.id2, condition2.corner.id2);
            converter.bER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.cER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);

            a = EdgeIndexLength(condition1.corner.id2, condition2.corner.id2);
            b = EdgeReferLength(converter.bER);
            c = EdgeReferLength(converter.cER);
            alpha = condition1.angle;
            beta = condition2.angle;

            judge = BottomALA(a, alpha, beta, ref b, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }
        return false;
    }

    // LA -> LLA
    private bool ResolveBottomLA(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 2 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomAngleCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomAngleCondition condition2 = (BottomAngleCondition)bottomConditions[1];

            bool isLA = IsEdgeOppositeCorner(condition1.edge, condition2.corner);
            if (!isLA)
                return false;

            float a, b, c, alpha;

            converter.aER = new EdgeRefer(condition2.corner.id2, condition2.corner.id1);
            converter.bER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.cER = new EdgeRefer(condition2.corner.id2, condition2.corner.id3);

            b = condition1.length;
            alpha = condition2.angle;
            a = Mathf.Min(b / Mathf.Sin(alpha * Mathf.Deg2Rad), EdgeIndexLength(condition2.corner.id2, condition2.corner.id1));
            c = EdgeReferLength(converter.cER);

            judge = BottomLLA(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    // AL -> LAL
    private bool ResolveBottomAL(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 2 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomAngleCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomAngleCondition condition2 = (BottomAngleCondition)bottomConditions[1];

            bool isAL = IsEdgeAdjacentCorner(condition1.edge, condition2.corner);
            if (!isAL)
                return false;

            float a, b, c, alpha;

            int id3 = EdgeOppositeIndex(condition1.edge);

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.corner.id2, id3);
            converter.cER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);

            a = condition1.length;
            b = EdgeIndexLength(condition2.corner.id2, id3);
            c = EdgeReferLength(converter.cER);
            alpha = condition2.angle;

            judge = BottomLAL(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    // LLL
    private bool ResolveBottomLLL(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 3 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomLengthCondition && bottomConditions[2] is BottomLengthCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomLengthCondition condition2 = (BottomLengthCondition)bottomConditions[1];
            BottomLengthCondition condition3 = (BottomLengthCondition)bottomConditions[2];

            float a, b, c;

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.cER = new EdgeRefer(condition3.edge.id1, condition3.edge.id2);

            a = condition1.length;
            b = condition2.length;
            c = condition3.length;

            judge = BottomLLL(a, b, c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    // LAL
    private bool ResolveBottomLAL(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 3 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomLengthCondition && bottomConditions[2] is BottomAngleCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomLengthCondition condition2 = (BottomLengthCondition)bottomConditions[1];
            BottomAngleCondition condition3 = (BottomAngleCondition)bottomConditions[2];

            bool isLAL = IsEdgeAdjacentCorner(condition1.edge, condition3.corner) && IsEdgeAdjacentCorner(condition2.edge, condition3.corner);
            if (!isLAL)
                return false;

            float a, b, c, alpha;

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.cER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            a = condition1.length;
            b = condition2.length;
            c = EdgeReferLength(converter.cER);
            alpha = condition3.angle;

            judge = BottomLAL(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    // LLA
    private bool ResolveBottomLLA(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 3 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomLengthCondition && bottomConditions[2] is BottomAngleCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomLengthCondition condition2 = (BottomLengthCondition)bottomConditions[1];
            BottomAngleCondition condition3 = (BottomAngleCondition)bottomConditions[2];

            bool isLLA = IsEdgeOppositeCorner(condition1.edge, condition3.corner) || IsEdgeOppositeCorner(condition2.edge, condition3.corner);
            if (!isLLA)
                return false;

            if (IsEdgeOppositeCorner(condition1.edge, condition3.corner))
                SwapCondition<BottomLengthCondition>(ref condition1, ref condition2);

            float a, b, c, alpha;

            int id3 = EdgeOppositeIndex(condition1.edge);

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.cER = new EdgeRefer(condition3.corner.id2, id3);

            a = condition1.length;
            b = condition2.length;
            c = EdgeReferLength(converter.cER);
            alpha = condition3.angle;

            judge = BottomLLA(a, b, alpha, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    // ALA
    private bool ResolveBottomALA(BottomConverter converter, out bool judge)
    {
        judge = false;

        if (bottomConditions.Count == 3 &&
            bottomConditions[0] is BottomLengthCondition && bottomConditions[1] is BottomAngleCondition && bottomConditions[2] is BottomAngleCondition)
        {
            BottomLengthCondition condition1 = (BottomLengthCondition)bottomConditions[0];
            BottomAngleCondition condition2 = (BottomAngleCondition)bottomConditions[1];
            BottomAngleCondition condition3 = (BottomAngleCondition)bottomConditions[2];

            if (IsEdgeOppositeCorner(condition1.edge, condition2.corner) || IsEdgeOppositeCorner(condition1.edge, condition3.corner))
            {
                int index = EdgeOppositeIndex(new EdgeRefer(condition2.corner.id2, condition3.corner.id2));
                float angle = 180 - condition2.angle - condition3.angle;
                BottomAngleCondition conditionN = new BottomAngleCondition(condition2.corner.id2, index, condition3.corner.id2, angle);
                if (IsEdgeOppositeCorner(condition1.edge, condition2.corner))
                    condition2 = conditionN;
                else if (IsEdgeOppositeCorner(condition1.edge, condition3.corner))
                    condition3 = conditionN;
            }

            float a, b, c, alpha, beta;

            int id3 = EdgeOppositeIndex(condition1.edge);

            converter.aER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.bER = new EdgeRefer(condition2.corner.id2, id3);
            converter.cER = new EdgeRefer(condition3.corner.id2, id3);

            a = condition1.length;
            b = EdgeReferLength(converter.bER);
            c = EdgeReferLength(converter.cER);
            alpha = condition2.angle;
            beta = condition3.angle;

            judge = BottomALA(a, alpha, beta, ref b, ref c);

            ConverterSetABC(converter, a, b, c);

            return true;
        }

        return false;
    }

    #region Calculation
    private bool BottomLLL(float a, float b, float c)
    {
        bool assert = (a + b) > c && (a + c) > b && (b + c) > a;
        if (!Assert(assert, "LLL"))
            return false;

        return true;
    }

    // a 和 b 的夹角为 alpha
    // alpha < 180
    private bool BottomLAL(float a, float b, float alpha, ref float c)
    {
        bool assert = alpha < 180;
        if (!Assert(assert, "LAL"))
            return false;

        float alphaRad = alpha * Mathf.Deg2Rad;

        float cosa = Mathf.Cos(alphaRad);
        c = Mathf.Sqrt(a * a + b * b - 2 * a * b * cosa);


        return true;
    }

    // a 和 c 的夹角为 alpha
    // alpha < 180
    private bool BottomLLA(float a, float b, float alpha, ref float c)
    {
        bool assert = alpha < 180;
        if (!Assert(assert, "LLA"))
            return false;

        float alphaRad = alpha * Mathf.Deg2Rad;

        float sina = Mathf.Sin(alphaRad);
        float powsina = sina * sina;

        if (alpha > 90)
            c = Mathf.Sqrt(b * b - a * a * powsina) - Mathf.Sqrt(a * a - a * a * powsina);
        else
            c = Mathf.Sqrt(a * a - a * a * powsina) + Mathf.Sqrt(b * b - a * a * powsina);

        return true;
    }

    // a 和 b 的夹角为 alpha
    // a 和 c 的夹角为 beta
    // alpha + beta < 180
    private bool BottomALA(float a, float alpha, float beta, ref float b, ref float c)
    {
        bool assert = (alpha + beta) < 180;
        if (!Assert(assert, "LAL"))
            return false;

        float alphaRad = alpha * Mathf.Deg2Rad;
        float betaRad = beta * Mathf.Deg2Rad;

        float sina = Mathf.Sin(alphaRad);
        float sinb = Mathf.Sin(betaRad);
        float sinab = Mathf.Sin(alphaRad + betaRad);

        b = a * sinb / sinab;
        c = a * sina / sinab;

        return true;
    }

    private bool SetBottomVertex(float lenAB, float lenAC, float lenBC)
    {
        // Debug.Log("lenAB: " + lenAB + "  lenAC: " + lenAC + "  lenBC: " + lenBC);
        bool can = CanBottom(lenAB, lenAC, lenBC);

        if (!can)
        {
            Debug.LogWarning("Error SetBottomVertex");
            return false;
        }

        float cx = (lenAB * lenAB + lenAC * lenAC - lenBC * lenBC) / 2 / lenAB;
        float cy = Mathf.Sqrt(lenAC * lenAC - cx * cx);

        float ox = (cx + lenAB) / 3;
        float oy = cy / 3;

        // Debug.Log("A: " + Vector2.zero);
        // Debug.Log("B: " + new Vector2(lenAB, 0));
        // Debug.Log("C: " + new Vector2(cx, cy));

        geometry.SetUnitVector(TriPyramid.BOTTOM_A, new Vector3(-ox, 0, -oy));
        geometry.SetUnitVector(TriPyramid.BOTTOM_B, new Vector3(-ox + lenAB, 0, -oy));
        geometry.SetUnitVector(TriPyramid.BOTTOM_C, new Vector3(-ox + cx, 0, -oy + cy));

        return true;
    }

    #endregion

    #region Util

    private void SwapCondition<T>(ref T t1, ref T t2) where T : Condition
    {
        T tmp = t1;
        t1 = t2;
        t2 = tmp;
    }


    private bool CanBottom(float a, float b, float c)
    {
        return (a + b) > c && (a + c) > b && (b + c) > a;
    }

    private float EdgeIndexLength(int id1, int id2)
    {
        Vector3 v1 = geometry.UnitVector(id1);
        Vector3 v2 = geometry.UnitVector(id2);
        return Vector3.Magnitude(v1 - v2);
    }

    private float EdgeReferLength(EdgeRefer er)
    {
        return EdgeIndexLength(er.id1, er.id2);
    }

    // return degree
    private float CornerIndexAngle(int id1, int id2, int id3)
    {
        Vector3 v1 = geometry.UnitVector(id1);
        Vector3 v2 = geometry.UnitVector(id2);
        Vector3 v3 = geometry.UnitVector(id3);
        return Mathf.Abs(Vector3.Angle(v1 - v2, v3 - v2));
    }

    private int EdgeOppositeIndex(EdgeRefer er)
    {
        if (er == new EdgeRefer(TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_B))
            return TriPyramid.BOTTOM_C;
        else if (er == new EdgeRefer(TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_B;
        else if (er == new EdgeRefer(TriPyramid.BOTTOM_B, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_A;
        return -1;
    }

    private bool IsEdgeOppositeCorner(EdgeRefer er, CornerRefer cr)
    {
        return cr.id2 != er.id1 && cr.id2 != er.id2;
    }

    private bool IsEdgeAdjacentCorner(EdgeRefer er, CornerRefer cr)
    {
        return cr.id2 == er.id1 || cr.id2 == er.id2;
    }


    private bool Assert(bool condition, string message)
    {
        if (!condition)
            Debug.LogWarning("Error " + message);
        return condition;
    }

    #endregion
}