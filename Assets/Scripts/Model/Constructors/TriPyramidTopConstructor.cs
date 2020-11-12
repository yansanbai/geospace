using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TriPyramidTopConstructor : Constructor
{

    private List<TriPyramidTopCondition> topConditions;

    Vector3 pointA, pointB, pointC;

    float lenAB, lenAC, lenBC;

    public TriPyramidTopConstructor(Geometry geometry) : base(geometry)
    {
        this.geometry = geometry;
        topConditions = new List<TriPyramidTopCondition>();
    }

    public override bool AddCondition(Condition condition)
    {
        if (condition is TriPyramidTopCondition)
        {
            TriPyramidTopCondition addCondition = (TriPyramidTopCondition)condition;

            bool result = CheckAddCondition(addCondition);

            // if (!result)
            // {
            //     Debug.LogWarning("Error In Try");
            //     return false;
            // }

            topConditions.Add(addCondition);
            topConditions.Sort(ConditionComparer);
            RefreshBottom();

            // result = ResolveTop();

            // if (!result)
            // {
            //     topConditions.Remove(addCondition);
            //     return false;
            // }

            return result;
        }

        return false;
    }

    public override bool RemoveCondition(Condition condition)
    {
        if (condition is TriPyramidTopCondition)
            return topConditions.Remove((TriPyramidTopCondition)condition);

        return false;
    }

    public override void ClearConditions()
    {
        topConditions.Clear();
    }

    public bool Resolve()
    {
        if (topConditions.Count == 0)
            return true;

        TopConverter converter;
        if (ResolveTop(out converter))
            return SetTop(converter);
        return false;
    }

    private bool CheckAddCondition(TriPyramidTopCondition condition)
    {
        // TODO


        return true;
    }

    private int ConditionComparer(TriPyramidTopCondition c1, TriPyramidTopCondition c2)
    {
        return c1.order.CompareTo(c2.order);
    }

    private class TopConverter
    {
        public float d, e, f;
        public EdgeRefer dER, eER, fER;
        public float lenSA, lenSB, lenSC;

        public void Resolve()
        {
            if (dER == new EdgeRefer(0, 3))
                lenSA = d;
            else if (eER == new EdgeRefer(0, 3))
                lenSA = e;
            else if (fER == new EdgeRefer(0, 3))
                lenSA = f;

            if (dER == new EdgeRefer(1, 3))
                lenSB = d;
            else if (eER == new EdgeRefer(1, 3))
                lenSB = e;
            else if (fER == new EdgeRefer(1, 3))
                lenSB = f;

            if (dER == new EdgeRefer(2, 3))
                lenSC = d;
            else if (eER == new EdgeRefer(2, 3))
                lenSC = e;
            else if (fER == new EdgeRefer(2, 3))
                lenSC = f;
        }
    }

    private void ConverterGetABC(TopConverter converter, out float a, out float b, out float c)
    {
        FormatEdgeRefer(ref converter.dER);
        FormatEdgeRefer(ref converter.eER);
        FormatEdgeRefer(ref converter.fER);
        a = EdgeIndexLength(converter.dER.id2, converter.eER.id2);
        b = EdgeIndexLength(converter.dER.id2, converter.fER.id2);
        c = EdgeIndexLength(converter.eER.id2, converter.fER.id2);
    }

    private void ConverterSetDEF(TopConverter converter, float d, float e, float f)
    {
        converter.d = d;
        converter.e = e;
        converter.f = f;
    }

    private delegate bool ResolveTopDelegate(TopConverter converter, out bool judge);


    private bool ResolveTop(out TopConverter converter)
    {
        converter = new TopConverter();

        ResolveTopDelegate[] resolves1 = {
            ResolveTopL,
            ResolveTopT,
            ResolveTopB,
        };

        ResolveTopDelegate[] resolves2 = {
            ResolveTopLL,
            ResolveTopTT,
            ResolveTopBBX,
            ResolveTopBBY,
            ResolveTopBBZ,
            ResolveTopLT,
            ResolveTopTL,
            ResolveTopBLX,
            ResolveTopBLY,
            ResolveTopBLZ,
            ResolveTopTB,
            ResolveTopBT,
        };

        ResolveTopDelegate[] resolves3 = {
            ResolveTopBBB,
            ResolveTopTTB,

            ResolveTopLLL,
            ResolveTopTTT,
            ResolveTopTLT,
            ResolveTopTTL,
            ResolveTopTLL,
            ResolveTopBLL,
            ResolveTopLLB,
            ResolveTopBBTX,
            ResolveTopBBTY,
            ResolveTopBBTZ,
            ResolveTopBLBY,
            ResolveTopBBLX,
            ResolveTopBLBX,
            ResolveTopBBLY,
            ResolveTopLBBZ,
            ResolveTopBLBZ,
            ResolveTopBBLZ,
            ResolveTopLBT,
            ResolveTopBLT,
            ResolveTopBTL,
            ResolveTopLTB,
            ResolveTopTLB,
            ResolveTopTBL,
        };

        ResolveTopDelegate[] resolves4 = {
            ResolveTopLTTB,
            ResolveTopTTLB,
            ResolveTopTTBL,
            ResolveTopLBBB,
            ResolveTopBBLB,
            ResolveTopBBBL,
        };

        ResolveTopDelegate[][] resolvesArray = {
            resolves1,
            resolves2,
            resolves3,
            resolves4
        };

        ResolveTopDelegate[] resolves = null;
        for (int i = 0; i < resolvesArray.Length; i++)
        {
            if (topConditions.Count == i + 1)
            {
                resolves = resolvesArray[i];
                break;
            }
        }

        if (resolves == null)
            return false;

        foreach (ResolveTopDelegate resolve in resolves)
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

    private bool SetTop(TopConverter converter)
    {
        if (converter == null)
            return false;
        return SetTopVertex(converter.lenSA, converter.lenSB, converter.lenSC);
    }

    // L  ->  LLL
    private bool ResolveTopL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 1 &&
            topConditions[0] is TopLengthCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];

            FormatEdgeRefer(ref condition1.edge);

            EdgeRefer bottom = EdgeOppositeBottom(condition1.edge);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, bottom.id1);
            converter.fER = new EdgeRefer(condition1.edge.id1, bottom.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            if (!CanTop(a, b, c, d, e, f))
            {
                float angle = CornerIndexAngle(condition1.edge.id1, condition1.edge.id2, bottom.id1);
                TriLAL(a, d, angle, out e);
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopLLL(a, b, c, d, e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // T  ->  TLL
    private bool ResolveTopT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 1 &&
            topConditions[0] is TopTRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];

            float a, b, c, d, e, f;

            int id3 = TCornerOppositeIndex(condition1.corner);

            converter.dER = new EdgeRefer(condition1.corner.id2, condition1.corner.id1);
            converter.eER = new EdgeRefer(condition1.corner.id2, condition1.corner.id3);
            converter.fER = new EdgeRefer(condition1.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTLL(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                d = FitLengthOfRange(0, a);
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopTLL(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // B  ->  LLB
    private bool ResolveTopB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 1 &&
            topConditions[0] is TopBRightCondition)
        {
            TopBRightCondition condition1 = (TopBRightCondition)topConditions[0];

            FormatBCornerRefer(ref condition1.corner);

            float a, b, c, d, e, f;

            int id3 = BCornerOppositeIndex(condition1.corner);

            converter.dER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.eER = new EdgeRefer(condition1.corner.id1, condition1.corner.id3);
            converter.fER = new EdgeRefer(condition1.corner.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLLB(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopLLB(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LL  ->  LLL
    private bool ResolveTopLL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopLengthCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopLengthCondition condition2 = (TopLengthCondition)topConditions[1];

            FormatEdgeRefer(ref condition1.edge);
            FormatEdgeRefer(ref condition2.edge);

            float a, b, c, d, e, f;

            int id3 = EdgeOppositeIndex(new EdgeRefer(condition1.edge.id2, condition2.edge.id2));

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = condition2.length;
            f = EdgeReferLength(converter.fER);

            if (!CanTop(a, b, c, d, e, f))
            {
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopLLL(a, b, c, d, e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TT  ->  TLT
    private bool ResolveTopTT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopTRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];

            float a, b, c, d, e, f;

            int id1 = TCornersAdjacentIndex(condition1.corner, condition2.corner);
            int id2 = TCornerOppositeIndex(condition1.corner);
            int id3 = TCornerOppositeIndex(condition2.corner);

            converter.dER = new EdgeRefer(condition1.corner.id2, id1);
            converter.eER = new EdgeRefer(condition1.corner.id2, id2);
            converter.fER = new EdgeRefer(condition2.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTLT(a, b, c, d, ref e, ref f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                d = FitLengthOfRange(0, Mathf.Min(a, b));
            }

            judge = TopTLT(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBY  ->  BLBY
    private bool ResolveTopBBY(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopBRightCondition && topConditions[1] is TopBRightCondition)
        {
            TopBRightCondition condition1 = (TopBRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBBY = IsBCornersRelative(condition1.corner, condition2.corner);
            if (!isBBY)
                return false;

            FormatBCornerRefer(ref condition1.corner);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.eER = new EdgeRefer(condition1.corner.id1, condition1.corner.id3);
            converter.fER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBLBY(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBX  ->  BLBX
    private bool ResolveTopBBX(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopBRightCondition && topConditions[1] is TopBRightCondition)
        {
            TopBRightCondition condition1 = (TopBRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBBX = IsBCornersContrary(condition1.corner, condition2.corner);
            if (!isBBX)
                return false;

            FormatBCornerRefer(ref condition1.corner);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.corner.id1, condition1.corner.id3);
            converter.eER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.fER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBLBX(a, b, c, d, ref e, ref f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                d = Mathf.Max(Mathf.Sqrt(a * a + e * e), Mathf.Sqrt(b * b + f * f));
            }

            judge = TopBLBX(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBZ  ->  LBBZ
    private bool ResolveTopBBZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopBRightCondition && topConditions[1] is TopBRightCondition)
        {
            TopBRightCondition condition1 = (TopBRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBBZ = IsBCornersParallel(condition1.corner, condition2.corner) || IsBCornersParallel(condition2.corner, condition1.corner);
            if (!isBBZ)
                return false;

            if (!IsBCornersParallel(condition1.corner, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition1, ref condition2);

            FormatBCornerRefer(ref condition1.corner);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.eER = new EdgeRefer(condition1.corner.id1, condition1.corner.id2);
            converter.fER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLBBZ(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LT  ->  TLL
    private bool ResolveTopLT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];

            bool isLT = IsEdgeOppositeTCorner(condition1.edge, condition2.corner);
            if (!isLT)
                return false;

            FormatEdgeRefer(ref condition1.edge);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id2, condition2.corner.id1);
            converter.eER = new EdgeRefer(condition2.corner.id2, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopTLL(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                float min = Mathf.Abs(b - f);
                float max = Mathf.Min(a, b + f);
                d = FitLengthOfRange(min, max);
            }

            judge = TopTLL(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TL  ->  TLL
    private bool ResolveTopTL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];

            bool isTL = IsEdgeAdjacentTCorner(condition1.edge, condition2.corner);
            if (!isTL)
                return false;

            FormatEdgeRefer(ref condition1.edge);

            float a, b, c, d, e, f;

            int id2 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);
            int id3 = TCornerOppositeIndex(condition2.corner);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, id2);
            converter.fER = new EdgeRefer(condition2.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTLL(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopTLL(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLY  ->  LLB
    private bool ResolveTopBLY(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBLY = IsEdgeForwardBCorner(condition1.edge, condition2.corner);
            if (!isBLY)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            int id3 = BCornerOppositeIndex(condition2.corner);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition1.edge.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLLB(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopLLB(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLX  ->  LLB
    private bool ResolveTopBLX(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBLX = IsEdgeOppositeBCorner(condition1.edge, condition2.corner);
            if (!isBLX)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopLLB(a, b, c, d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                float angle = CornerIndexAngle(condition1.edge.id1, condition1.edge.id2, condition2.corner.id2);
                TriLAL(b, angle, f, out d);
            }

            judge = TopLLB(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLZ  ->  BLL
    private bool ResolveTopBLZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBLZ = IsEdgeBackwardBCorner(condition1.edge, condition2.corner);
            if (!isBLZ)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            int id3 = BCornerOppositeIndex(condition2.corner);

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBLL(a, b, c, ref d, e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                Range<float> range = TriPyramidEdgeRange(a, b, c, d, e);
                f = FitLengthOfRange(range.min, range.max);
            }

            judge = TopBLL(a, b, c, ref d, e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TB  ->  TBL
    private bool ResolveTopTB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isTB = IsBCornerAdjacentTCorner(condition2.corner, condition1.corner);
            if (!isTB)
                return false;

            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition1.corner, condition2.corner.id2);

            converter.dER = new EdgeRefer(condition1.corner.id2, id1);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTBL(a, b, c, ref d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                f = FitLengthOfRange(0, b);
            }

            judge = TopTBL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BT  ->  BTL
    private bool ResolveTopBT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 2 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];

            bool isBT = IsBCornerOppositeTCorner(condition2.corner, condition1.corner);
            if (!isBT)
                return false;

            FormatBCornerRefer(ref condition2.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition1.corner, condition2.corner.id3);

            converter.dER = new EdgeRefer(condition1.corner.id2, id1);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.fER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBTL(a, b, c, ref d, ref e, f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                f = FitLengthOfRange(c, b);
            }

            judge = TopBTL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBB  ->  LBBB
    private bool ResolveTopBBB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopBRightCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopBRightCondition condition1 = (TopBRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            if (IsBCornersRelative(condition1.corner, condition2.corner))
            { }
            else if (IsBCornersRelative(condition1.corner, condition3.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);
            else if (IsBCornersRelative(condition2.corner, condition3.corner))
                SwapCondition<TopBRightCondition>(ref condition1, ref condition3);
            else
                return false;

            if (!IsBCornersParallel(condition3.corner, condition1.corner))
                SwapCondition<TopBRightCondition>(ref condition1, ref condition2);

            FormatBCornerRefer(ref condition1.corner);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition1.corner.id1, condition1.corner.id3);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLBBB(a, b, c, d, ref e, ref f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                d = FitLengthOfRange(0, Mathf.Min(a, b));
            }

            judge = TopLBBB(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TTB  ->  TTLB
    private bool ResolveTopTTB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isTTB = IsBCornerAdjacentTCorner(condition3.corner, condition1.corner) || IsBCornerAdjacentTCorner(condition3.corner, condition2.corner);
            if (!isTTB)
                return false;

            if (!IsBCornerAdjacentTCorner(condition3.corner, condition1.corner))
                SwapCondition<TopTRightCondition>(ref condition1, ref condition2);

            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id3 = TCornerOtherIndex(condition1.corner, condition3.corner.id2);

            converter.dER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.fER = new EdgeRefer(condition3.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTTLB(a, b, c, ref d, e, ref f);

            if (!judge || !CanTop(a, b, c, d, e, f))
            {
                e = FitLengthOfRange(0, c);
            }

            judge = TopTTLB(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LLL
    private bool ResolveTopLLL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopLengthCondition && topConditions[2] is TopLengthCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopLengthCondition condition2 = (TopLengthCondition)topConditions[1];
            TopLengthCondition condition3 = (TopLengthCondition)topConditions[2];

            FormatEdgeRefer(ref condition1.edge);
            FormatEdgeRefer(ref condition2.edge);
            FormatEdgeRefer(ref condition3.edge);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);
            converter.fER = new EdgeRefer(condition3.edge.id1, condition3.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = condition2.length;
            f = condition3.length;

            judge = TopLLL(a, b, c, d, e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TTT
    private bool ResolveTopTTT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];

            float a, b, c, d, e, f;

            int id1 = TCornerOppositeIndex(condition1.corner);
            int id2 = TCornerOppositeIndex(condition2.corner);
            int id3 = TCornerOppositeIndex(condition3.corner);

            converter.dER = new EdgeRefer(condition1.corner.id2, id1);
            converter.eER = new EdgeRefer(condition2.corner.id2, id2);
            converter.fER = new EdgeRefer(condition3.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTTT(a, b, c, ref d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TLT
    private bool ResolveTopTLT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];

            bool isTLT = IsEdgeAdjacentTCorner(condition1.edge, condition2.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition3.corner);
            if (!isTLT)
                return false;

            FormatEdgeRefer(ref condition1.edge);

            float a, b, c, d, e, f;

            int id2 = TCornerOppositeIndex(condition2.corner);
            int id3 = TCornerOppositeIndex(condition3.corner);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopTLT(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }
        return false;
    }

    // TTL
    private bool ResolveTopTTL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];

            bool isTTL = IsEdgeAdjacentTCorner(condition1.edge, condition2.corner) || IsEdgeAdjacentTCorner(condition1.edge, condition3.corner);
            if (!isTTL)
                return false;

            if (!IsEdgeAdjacentTCorner(condition1.edge, condition2.corner))
                SwapCondition<TopTRightCondition>(ref condition2, ref condition3);

            FormatEdgeRefer(ref condition1.edge);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);
            int id3 = TCornerOtherIndex(condition3.corner, id1);

            converter.dER = new EdgeRefer(condition1.edge.id1, id1);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopTTL(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TLL
    private bool ResolveTopTLL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopLengthCondition && topConditions[2] is TopTRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopLengthCondition condition2 = (TopLengthCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];

            bool isTLL = (IsEdgeAdjacentTCorner(condition1.edge, condition3.corner) && IsEdgeOppositeTCorner(condition2.edge, condition3.corner))
            || (IsEdgeAdjacentTCorner(condition2.edge, condition3.corner) && IsEdgeOppositeTCorner(condition1.edge, condition3.corner));
            if (!isTLL)
                return false;

            if (!IsEdgeAdjacentTCorner(condition1.edge, condition3.corner))
                SwapCondition<TopLengthCondition>(ref condition1, ref condition2);

            FormatEdgeRefer(ref condition1.edge);
            FormatEdgeRefer(ref condition2.edge);

            float a, b, c, d, e, f;

            int id2 = TCornerOtherIndex(condition3.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, id2);
            converter.fER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = condition2.length;

            judge = TopTLL(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLL
    private bool ResolveTopBLL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopLengthCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopLengthCondition condition2 = (TopLengthCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBLL = (IsEdgeBackwardBCorner(condition1.edge, condition3.corner) && IsEdgeOppositeBCorner(condition2.edge, condition3.corner))
            || (IsEdgeOppositeBCorner(condition2.edge, condition3.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner));
            if (!isBLL)
                return false;

            if (!IsEdgeBackwardBCorner(condition1.edge, condition3.corner))
                SwapCondition<TopLengthCondition>(ref condition1, ref condition2);

            FormatEdgeRefer(ref condition1.edge);
            FormatEdgeRefer(ref condition2.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = condition2.length;

            judge = TopBLL(a, b, c, ref d, e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LLB
    private bool ResolveTopLLB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopLengthCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopLengthCondition condition2 = (TopLengthCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isLLB = (IsEdgeForwardBCorner(condition1.edge, condition3.corner) && IsEdgeOppositeBCorner(condition2.edge, condition3.corner))
            || (IsEdgeOppositeBCorner(condition2.edge, condition3.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner));
            if (!isLLB)
                return false;

            if (!IsEdgeForwardBCorner(condition1.edge, condition3.corner))
                SwapCondition<TopLengthCondition>(ref condition1, ref condition2);

            FormatEdgeRefer(ref condition1.edge);
            FormatEdgeRefer(ref condition2.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);
            converter.fER = new EdgeRefer(condition2.edge.id1, condition2.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = condition2.length;

            judge = TopLLB(a, b, c, d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBTY
    private bool ResolveTopBBTY(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBTY = IsBCornersRelative(condition2.corner, condition3.corner) && IsTCornerStaggerBCorner(condition1.corner, condition2.corner) && IsTCornerStaggerBCorner(condition1.corner, condition3.corner);
            if (!isBBTY)
                return false;

            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.corner.id2, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBBTY(a, b, c, ref d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBTX
    private bool ResolveTopBBTX(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBTX = IsBCornersContrary(condition2.corner, condition3.corner) && IsTCornerStaggerBCorner(condition1.corner, condition2.corner) && IsTCornerStaggerBCorner(condition1.corner, condition3.corner);
            if (!isBBTX)
                return false;

            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.corner.id2, condition2.corner.id3);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBBTX(a, b, c, ref d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBTZ
    private bool ResolveTopBBTZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopTRightCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopTRightCondition condition1 = (TopTRightCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBTZ = (IsBCornersParallel(condition2.corner, condition3.corner) || IsBCornersParallel(condition3.corner, condition2.corner))
            && IsTCornerStaggerBCorner(condition1.corner, condition2.corner) && IsTCornerStaggerBCorner(condition1.corner, condition3.corner);
            if (!isBBTZ)
                return false;

            if (!IsBCornersParallel(condition2.corner, condition3.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);

            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.fER = new EdgeRefer(condition1.corner.id2, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBBTZ(a, b, c, ref d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLBY
    private bool ResolveTopBLBY(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBLBY = IsBCornersRelative(condition2.corner, condition3.corner) && IsEdgeForwardBCorner(condition1.edge, condition2.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner);
            if (!isBLBY)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBLBY(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBLY
    private bool ResolveTopBBLY(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBLY = IsBCornersRelative(condition2.corner, condition3.corner) && (IsEdgeBackwardBCorner(condition1.edge, condition2.corner) || IsEdgeBackwardBCorner(condition1.edge, condition3.corner));
            if (!isBBLY)
                return false;

            if (!IsEdgeBackwardBCorner(condition1.edge, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);


            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBBLY(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLBX
    private bool ResolveTopBLBX(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBLBX = IsBCornersContrary(condition2.corner, condition3.corner) && IsEdgeBackwardBCorner(condition1.edge, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner);
            if (!isBLBX)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopBLBX(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBLX
    private bool ResolveTopBBLX(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBLX = IsBCornersContrary(condition2.corner, condition3.corner) && (IsEdgeForwardBCorner(condition1.edge, condition2.corner) || IsEdgeForwardBCorner(condition1.edge, condition3.corner));
            if (!isBBLX)
                return false;

            if (!IsEdgeForwardBCorner(condition1.edge, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);


            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBBLX(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LBBZ
    private bool ResolveTopLBBZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isLBBZ = (IsBCornersParallel(condition2.corner, condition3.corner) && IsEdgeForwardBCorner(condition1.edge, condition2.corner))
            || (IsBCornersParallel(condition3.corner, condition2.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner));
            if (!isLBBZ)
                return false;

            if (!IsEdgeForwardBCorner(condition1.edge, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);


            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLBBZ(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLBZ
    private bool ResolveTopBLBZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBLBZ = (IsBCornersParallel(condition2.corner, condition3.corner) && IsEdgeBackwardBCorner(condition1.edge, condition2.corner))
            || (IsBCornersParallel(condition3.corner, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner));
            if (!isBLBZ)
                return false;

            if (!IsEdgeBackwardBCorner(condition1.edge, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);


            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBLBZ(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBLZ
    private bool ResolveTopBBLZ(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBBLZ = (IsBCornersParallel(condition2.corner, condition3.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner))
            || (IsBCornersParallel(condition3.corner, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition2.corner));
            if (!isBBLZ)
                return false;

            if (!IsEdgeBackwardBCorner(condition1.edge, condition3.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);


            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopBBLZ(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LBT
    private bool ResolveTopLBT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isLBT = IsBCornerOppositeTCorner(condition3.corner, condition2.corner) && IsEdgeOppositeBCorner(condition1.edge, condition3.corner);
            if (!isLBT)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id3 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.fER = new EdgeRefer(condition2.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLBT(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BLT
    private bool ResolveTopBLT(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBLT = IsBCornerOppositeTCorner(condition3.corner, condition2.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner);
            if (!isBLT)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition2.corner, condition3.corner.id3);

            converter.dER = new EdgeRefer(condition2.corner.id2, id1);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBLT(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BTL
    private bool ResolveTopBTL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isBTL = IsBCornerOppositeTCorner(condition3.corner, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner);
            if (!isBTL)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition2.corner.id2, id1);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopBTL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LTB
    private bool ResolveTopLTB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isLTB = IsBCornerAdjacentTCorner(condition3.corner, condition2.corner) && IsEdgeOppositeBCorner(condition1.edge, condition3.corner);
            if (!isLTB)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id3 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);
            converter.fER = new EdgeRefer(condition2.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLTB(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TLB
    private bool ResolveTopTLB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isTLB = IsBCornerAdjacentTCorner(condition3.corner, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner);
            if (!isTLB)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition2.corner, condition3.corner.id2);

            converter.dER = new EdgeRefer(condition2.corner.id2, id1);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopTLB(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TBL
    private bool ResolveTopTBL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 3 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];

            bool isTBL = IsBCornerAdjacentTCorner(condition3.corner, condition2.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner);
            if (!isTBL)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition3.corner);

            float a, b, c, d, e, f;

            int id1 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition2.corner.id2, id1);
            converter.eER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopTBL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LTTB
    private bool ResolveTopLTTB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            bool isLTTB = (IsBCornerAdjacentTCorner(condition4.corner, condition2.corner) && IsEdgeOppositeTCorner(condition1.edge, condition2.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition3.corner))
            || (IsBCornerAdjacentTCorner(condition4.corner, condition3.corner) && IsEdgeOppositeTCorner(condition1.edge, condition3.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition2.corner));
            if (!isLTTB)
                return false;

            if (!IsBCornerAdjacentTCorner(condition4.corner, condition2.corner))
                SwapCondition<TopTRightCondition>(ref condition2, ref condition3);

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            int id3 = TCornerOtherIndex(condition3.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition4.corner.id1, condition4.corner.id2);
            converter.fER = new EdgeRefer(condition3.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLTTB(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TTLB
    private bool ResolveTopTTLB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            bool isTTLB = (IsBCornerAdjacentTCorner(condition4.corner, condition2.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition2.corner) && IsEdgeOppositeTCorner(condition1.edge, condition3.corner))
            || (IsBCornerAdjacentTCorner(condition4.corner, condition3.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition3.corner) && IsEdgeOppositeTCorner(condition1.edge, condition2.corner));
            if (!isTTLB)
                return false;

            if (!IsBCornerAdjacentTCorner(condition4.corner, condition2.corner))
                SwapCondition<TopTRightCondition>(ref condition2, ref condition3);

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            int id3 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition4.corner.id1, condition4.corner.id3);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id2, id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopTTLB(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // TTBL
    private bool ResolveTopTTBL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopTRightCondition && topConditions[2] is TopTRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopTRightCondition condition2 = (TopTRightCondition)topConditions[1];
            TopTRightCondition condition3 = (TopTRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            bool isTTBL = (IsBCornerAdjacentTCorner(condition4.corner, condition2.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition2.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition3.corner))
            || (IsBCornerAdjacentTCorner(condition4.corner, condition3.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition3.corner) && IsEdgeAdjacentTCorner(condition1.edge, condition2.corner));
            if (!isTTBL)
                return false;

            if (!IsBCornerAdjacentTCorner(condition4.corner, condition2.corner))
                SwapCondition<TopTRightCondition>(ref condition2, ref condition3);

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            int id2 = TCornerOtherIndex(condition2.corner, condition1.edge.id2);

            converter.dER = new EdgeRefer(condition4.corner.id1, condition4.corner.id3);
            converter.eER = new EdgeRefer(condition2.corner.id2, id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopTTBL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // LBBB
    private bool ResolveTopLBBB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            if (IsBCornersRelative(condition2.corner, condition3.corner))
            { }
            else if (IsBCornersRelative(condition2.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition3, ref condition4);
            else if (IsBCornersRelative(condition3.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition4);
            else
                return false;

            if (!IsBCornersParallel(condition4.corner, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);

            bool isLBBB = IsEdgeForwardBCorner(condition1.edge, condition2.corner) && IsEdgeForwardBCorner(condition1.edge, condition3.corner) && IsEdgeOppositeBCorner(condition1.edge, condition4.corner);
            if (!isLBBB)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.eER = new EdgeRefer(condition2.corner.id1, condition2.corner.id3);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = condition1.length;
            e = EdgeReferLength(converter.eER);
            f = EdgeReferLength(converter.fER);

            judge = TopLBBB(a, b, c, d, ref e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBLB
    private bool ResolveTopBBLB(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            if (IsBCornersRelative(condition2.corner, condition3.corner))
            { }
            else if (IsBCornersRelative(condition2.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition3, ref condition4);
            else if (IsBCornersRelative(condition3.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition4);
            else
                return false;

            if (!IsBCornersParallel(condition4.corner, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);

            bool isBBLB = IsEdgeBackwardBCorner(condition1.edge, condition2.corner) && IsEdgeOppositeBCorner(condition1.edge, condition3.corner) && IsEdgeForwardBCorner(condition1.edge, condition4.corner);
            if (!isBBLB)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);
            converter.fER = new EdgeRefer(condition3.corner.id1, condition3.corner.id3);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = condition1.length;
            f = EdgeReferLength(converter.fER);

            judge = TopBBLB(a, b, c, ref d, e, ref f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    // BBBL
    private bool ResolveTopBBBL(TopConverter converter, out bool judge)
    {
        judge = false;

        if (topConditions.Count == 4 &&
            topConditions[0] is TopLengthCondition && topConditions[1] is TopBRightCondition && topConditions[2] is TopBRightCondition && topConditions[3] is TopBRightCondition)
        {
            TopLengthCondition condition1 = (TopLengthCondition)topConditions[0];
            TopBRightCondition condition2 = (TopBRightCondition)topConditions[1];
            TopBRightCondition condition3 = (TopBRightCondition)topConditions[2];
            TopBRightCondition condition4 = (TopBRightCondition)topConditions[3];

            if (IsBCornersRelative(condition2.corner, condition3.corner))
            { }
            else if (IsBCornersRelative(condition2.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition3, ref condition4);
            else if (IsBCornersRelative(condition3.corner, condition4.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition4);
            else
                return false;

            if (!IsBCornersParallel(condition4.corner, condition2.corner))
                SwapCondition<TopBRightCondition>(ref condition2, ref condition3);

            bool isBBBL = IsEdgeOppositeBCorner(condition1.edge, condition2.corner) && IsEdgeBackwardBCorner(condition1.edge, condition3.corner) && IsEdgeBackwardBCorner(condition1.edge, condition4.corner);
            if (!isBBBL)
                return false;

            FormatEdgeRefer(ref condition1.edge);
            FormatBCornerRefer(ref condition2.corner);
            FormatBCornerRefer(ref condition3.corner);
            FormatBCornerRefer(ref condition4.corner);

            float a, b, c, d, e, f;

            converter.dER = new EdgeRefer(condition2.corner.id1, condition2.corner.id2);
            converter.eER = new EdgeRefer(condition4.corner.id1, condition4.corner.id2);
            converter.fER = new EdgeRefer(condition1.edge.id1, condition1.edge.id2);

            ConverterGetABC(converter, out a, out b, out c);

            d = EdgeReferLength(converter.dER);
            e = EdgeReferLength(converter.eER);
            f = condition1.length;

            judge = TopBBBL(a, b, c, ref d, ref e, f);

            ConverterSetDEF(converter, d, e, f);

            return true;
        }

        return false;
    }

    #region Calculation

    // a - AB    b - AC    c - BC
    // d - SA    e - SB    f - SC
    // a - de    b - df    c - ef
    private bool TopTTT(float a, float b, float c, ref float d, ref float e, ref float f)
    {
        float powA = a * a;
        float powB = b * b;
        float powC = c * c;

        d = Mathf.Sqrt((powA + powB - powC) / 2);
        e = Mathf.Sqrt((powA + powC - powB) / 2);
        f = Mathf.Sqrt((powB + powC - powA) / 2);

        return true;
    }

    // d ⊥ e
    // d ⊥ f
    // d 已知
    // a - de    b - df    c - ef
    // a2 > d2    b2 > d2
    private bool TopTLT(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = (a * a) > (d * d) && (b * b) > (d * d);
        if (!Assert(assert, "LTT"))
            return false;

        e = Mathf.Sqrt(a * a - d * d);
        f = Mathf.Sqrt(b * b - d * d);
        return true;
    }

    // d ⊥ e
    // d ⊥ f
    // e 已知
    // a - de    b - df    c - ef
    // b2 + e2 > a2
    private bool TopTTL(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (b * b + e * e) > (a * a);
        if (!Assert(assert, "LTT"))
            return false;

        d = Mathf.Sqrt(a * a - e * e);
        f = Mathf.Sqrt(b * b - d * d);
        return true;
    }

    // d ⊥ e
    // d 已知
    // f 已知
    // a - de    b - df    c - ef
    // a2 > d2
    private bool TopTLL(float a, float b, float c, float d, ref float e, float f)
    {
        bool assert = (a * a) > (d * d);
        if (!Assert(assert, "TLL"))
            return false;

        e = Mathf.Sqrt(a * a - d * d);
        return true;
    }

    // d ⊥ a
    // e 已知
    // f 已知
    // a - de    b - df    c - ef
    // a2 < e2
    private bool TopBLL(float a, float b, float c, ref float d, float e, float f)
    {
        bool assert = (a * a) < (e * e);
        if (!Assert(assert, "BLL"))
            return false;

        d = Mathf.Sqrt(e * e - a * a);
        return true;
    }

    // d ⊥ a
    // d 已知
    // f 已知
    // a - de    b - df    c - ef
    private bool TopLLB(float a, float b, float c, float d, ref float e, float f)
    {
        e = Mathf.Sqrt(a * a + d * d);
        return true;
    }

    // d ⊥ a
    // d ⊥ b
    // e ⊥ f
    // a - de    b - df    c - ef
    // a2 + b2 < c2
    private bool TopBBTY(float a, float b, float c, ref float d, ref float e, ref float f)
    {
        bool assert = (a * a + b * b) < (c * c);
        if (!Assert(assert, "BBTY"))
            return false;

        d = Mathf.Sqrt((c * c - a * a - b * b) / 2.0f);
        e = Mathf.Sqrt(a * a + d * d);
        f = Mathf.Sqrt(b * b + d * d);
        return true;
    }

    // e ⊥ a
    // f ⊥ b
    // e ⊥ f
    // a - de    b - df    c - ef
    private bool TopBBTX(float a, float b, float c, ref float d, ref float e, ref float f)
    {
        d = Mathf.Sqrt((a * a + b * b + c * c) / 2.0f);
        e = Mathf.Sqrt(d * d - a * a);
        f = Mathf.Sqrt(d * d - b * b);
        return true;
    }

    // d ⊥ a
    // e ⊥ c
    // d ⊥ f
    // a - de    b - df    c - ef
    // a2 + c2 < b2
    private bool TopBBTZ(float a, float b, float c, ref float d, ref float e, ref float f)
    {
        bool assert = (a * a + c * c) < (b * b);
        if (!Assert(assert, "BBTZ"))
            return false;

        e = Mathf.Sqrt((a * a + b * b - c * c) / 2.0f);
        d = Mathf.Sqrt(e * e - a * a);
        f = Mathf.Sqrt(c * c + e * e);
        return true;
    }

    // d ⊥ a
    // d ⊥ b
    // d 已知
    // a - de    b - df    c - ef
    private bool TopBLBY(float a, float b, float c, float d, ref float e, ref float f)
    {
        e = Mathf.Sqrt(a * a + d * d);
        f = Mathf.Sqrt(b * b + d * d);
        return true;
    }

    // d ⊥ a
    // d ⊥ b
    // e 已知
    // a - de    b - df    c - ef
    // a2 < e2
    private bool TopBBLY(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (a * a) < (e * e);
        if (!Assert(assert, "BBLY"))
            return false;

        d = Mathf.Sqrt(e * e - a * a);
        f = Mathf.Sqrt(b * b + d * d);
        return true;
    }

    // e ⊥ a
    // f ⊥ b
    // d 已知
    // a - de    b - df    c - ef
    // d2 > a2    d2 > b2
    private bool TopBLBX(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = (a * a) < (d * d) && (b * b) < (d * d);
        if (!Assert(assert, "BLBX"))
            return false;

        e = Mathf.Sqrt(d * d - a * a);
        f = Mathf.Sqrt(d * d - b * b);
        return true;
    }

    // e ⊥ a
    // f ⊥ b
    // e 已知
    // a - de    b - df    c - ef
    // a2 + e2 > b2
    private bool TopBBLX(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (a * a + e * e) > (b * b);
        if (!Assert(assert, "BBLX"))
            return false;

        d = Mathf.Sqrt(a * a + e * e);
        f = Mathf.Sqrt(d * d - b * b);
        return true;
    }

    // d ⊥ a
    // e ⊥ c
    // d 已知
    // a - de    b - df    c - ef
    private bool TopLBBZ(float a, float b, float c, float d, ref float e, ref float f)
    {
        e = Mathf.Sqrt(a * a + d * d);
        f = Mathf.Sqrt(c * c + e * e);
        return true;
    }

    // d ⊥ a
    // e ⊥ c
    // e 已知
    // a - de    b - df    c - ef
    // a2 < e2
    private bool TopBLBZ(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (a * a) < (e * e);
        if (!Assert(assert, "BLBZ"))
            return false;

        d = Mathf.Sqrt(e * e - a * a);
        f = Mathf.Sqrt(c * c + e * e);
        return true;
    }

    // d ⊥ a
    // e ⊥ c
    // f 已知
    // a - de    b - df    c - ef
    // a2 + c2 < f2
    private bool TopBBLZ(float a, float b, float c, ref float d, ref float e, float f)
    {
        bool assert = (a * a + c * c) < (f * f);
        if (!Assert(assert, "BBLZ"))
            return false;

        e = Mathf.Sqrt(f * f - c * c);
        d = Mathf.Sqrt(e * e - a * a);
        return true;
    }

    // e ⊥ c
    // d ⊥ f
    // d 已知
    // a - de    b - df    c - ef
    // c2 + d2 < b2
    private bool TopLBT(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = (c * c + d * d) < (b * b);
        if (!Assert(assert, "LBT"))
            return false;

        f = Mathf.Sqrt(b * b - d * d);
        e = Mathf.Sqrt(f * f - c * c);
        return true;
    }

    // e ⊥ c
    // d ⊥ f
    // e 已知
    // a - de    b - df    c - ef
    // c2 + e2 < b2
    private bool TopBLT(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (c * c + e * e) < (b * b);
        if (!Assert(assert, "BLT"))
            return false;

        f = Mathf.Sqrt(c * c + e * e);
        d = Mathf.Sqrt(b * b - f * f);
        return true;
    }

    // e ⊥ c
    // d ⊥ f
    // f 已知
    // a - de    b - df    c - ef
    // c2 < f2    f2 < b2
    private bool TopBTL(float a, float b, float c, ref float d, ref float e, float f)
    {
        bool assert = (c * c) < (f * f) && (f * f) < (b * b);
        if (!Assert(assert, "BTL"))
            return false;

        d = Mathf.Sqrt(b * b - f * f);
        e = Mathf.Sqrt(f * f - c * c);
        return true;
    }

    // f ⊥ c
    // d ⊥ f
    // d 已知
    // a - de    b - df    c - ef
    // b2 > d2
    private bool TopLTB(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = (b * b) > (d * d);
        if (!Assert(assert, "LTB"))
            return false;

        f = Mathf.Sqrt(b * b - d * d);
        e = Mathf.Sqrt(c * c + f * f);
        return true;
    }

    // f ⊥ c
    // d ⊥ f
    // e 已知
    // a - de    b - df    c - ef
    // c2 < e2    b2 + c2 > e2
    private bool TopTLB(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = (c * c) < (e * e) && (b * b + c * c) > (e * e);
        if (!Assert(assert, "TLB"))
            return false;

        f = Mathf.Sqrt(e * e - c * c);
        d = Mathf.Sqrt(b * b - f * f);
        return true;
    }

    // f ⊥ c
    // d ⊥ f
    // f 已知
    // a - de    b - df    c - ef
    // b2 > f2
    private bool TopTBL(float a, float b, float c, ref float d, ref float e, float f)
    {
        bool assert = (b * b) > (f * f);
        if (!Assert(assert, "TBL"))
            return false;

        d = Mathf.Sqrt(b * b - f * f);
        e = Mathf.Sqrt(c * c + f * f);
        return true;
    }

    // TODO BBB + L     TTB + L

    // d ⊥ f
    // e ⊥ f
    // e ⊥ a
    // d 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2    a2 < d2    b2 > d2
    private bool TopLTTB(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b) && (a * a) < (d * d) && (b * b) > (d * d);
        if (!Assert(assert, "LTTB"))
            return false;

        e = Mathf.Sqrt(d * d - a * a);
        f = Mathf.Sqrt(b * b - d * d);
        return true;
    }

    // d ⊥ f
    // e ⊥ f
    // d ⊥ a
    // e 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2    c2 > e2
    private bool TopTTLB(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b) && (c * c) > (e * e);
        if (!Assert(assert, "TTLB"))
            return false;

        d = Mathf.Sqrt(a * a + e * e);
        f = Mathf.Sqrt(c * c - e * e);
        return true;
    }

    // d ⊥ f
    // e ⊥ f
    // d ⊥ a
    // f 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2    b2 > f2    c2 > f2
    private bool TopTTBL(float a, float b, float c, ref float d, ref float e, float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b) && (b * b) > (f * f) && (c * c) > (f * f);
        if (!Assert(assert, "TTBL"))
            return false;

        d = Mathf.Sqrt(b * b - f * f);
        e = Mathf.Sqrt(c * c - f * f);
        return true;
    }


    // d ⊥ a
    // d ⊥ b
    // e ⊥ c
    // d 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2
    private bool TopLBBB(float a, float b, float c, float d, ref float e, ref float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b);
        if (!Assert(assert, "LBBB"))
            return false;

        e = Mathf.Sqrt(a * a + d * d);
        f = Mathf.Sqrt(b * b + d * d);
        return true;
    }

    // d ⊥ a
    // d ⊥ b
    // e ⊥ c
    // e 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2    a2 < e2
    private bool TopBBLB(float a, float b, float c, ref float d, float e, ref float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b) && (a * a) < (e * e);
        if (!Assert(assert, "BBLB"))
            return false;

        d = Mathf.Sqrt(e * e - a * a);
        f = Mathf.Sqrt(c * c + e * e);
        return true;
    }

    // d ⊥ a
    // d ⊥ b
    // e ⊥ c
    // f 已知
    // a - de    b - df    c - ef
    // a2 + c2 = b2    b2 < f2    c2 < f2
    private bool TopBBBL(float a, float b, float c, ref float d, ref float e, float f)
    {
        bool assert = Math.AboutEquals(a * a + c * c, b * b) && (b * b) < (f * f) && (c * c) < (f * f);
        if (!Assert(assert, "BBBL"))
            return false;

        d = Mathf.Sqrt(f * f - b * b);
        e = Mathf.Sqrt(f * f - c * c);
        return true;
    }

    private bool TopLLL(float a, float b, float c, float d, float e, float f)
    {
        return true;
    }

    private void RefreshBottom()
    {
        pointA = geometry.UnitVector(TriPyramid.BOTTOM_A);
        pointB = geometry.UnitVector(TriPyramid.BOTTOM_B);
        pointC = geometry.UnitVector(TriPyramid.BOTTOM_C);

        lenAB = Vector3.Magnitude(pointA - pointB);
        lenAC = Vector3.Magnitude(pointA - pointC);
        lenBC = Vector3.Magnitude(pointB - pointC);
    }

    private bool SetTopVertex(float lenSA, float lenSB, float lenSC)
    {
        bool can = CanTop(lenAB, lenAC, lenBC, lenSA, lenSB, lenSC);

        if (!can)
        {
            Debug.LogWarning("Error SetTopVertex");
            return false;
        }

        Vector3 center;
        Vector3 direction;
        float r;

        can = Circle(pointA, pointB, lenSA, lenSB, out center, out direction, out r);
        if (!can)
        {
            Debug.LogWarning("Error SetTopVertex 1");
            return false;
        }

        Vector3 pointD = PointProjectOnPlane(pointC, center, direction);
        float lenCD = Vector3.Magnitude(pointD - pointC);
        float d = Vector3.Magnitude(pointD - center);
        float min = Mathf.Sqrt((r - d) * (r - d) + lenCD * lenCD);
        float max = Mathf.Sqrt((r + d) * (r + d) + lenCD * lenCD);

        can = lenSC < min || lenSC > max;
        if (can)
        {
            Debug.LogWarning("Error SetTopVertex 2");
            return false;
        }

        float e = Mathf.Sqrt(lenSC * lenSC - lenCD * lenCD);
        float tmp = (e * e - r * r - d * d) / 2 / d;
        float h = Mathf.Sqrt(r * r - tmp * tmp);


        Vector3 dirCenterD = Vector3.Normalize(pointD - center);
        Vector3 dirH = Vector3.Normalize(Vector3.Cross(direction, dirCenterD));

        float d_e = Mathf.Sqrt(e * e - h * h);
        Vector3 pointH = center + dirCenterD * (d - d_e);
        Vector3 pointS = pointH + dirH * h;
        // return pointS;
        geometry.SetUnitVector(TriPyramid.TOP_S, pointS);

        return true;
    }

    private bool Circle(Vector3 pointA, Vector3 pointB, float a, float b, out Vector3 center, out Vector3 direction, out float radius)
    {
        direction = Vector3.Normalize(pointA - pointB);
        float c = Vector3.Magnitude(pointA - pointB);

        if (c > (a + b))
        {
            center = Vector3.zero;
            radius = 0;
            return false;
        }

        float tmp = (a * a - b * b - c * c) / 2 / c;
        radius = Mathf.Sqrt(b * b - tmp * tmp);

        float d_a = Mathf.Sqrt(a * a - radius * radius);
        center = pointB + direction * (c - d_a);

        // Debug.Log(" radius: " + radius + "     AB: " + c + "    d_a: " + d_a + " center: " + center);

        return true;
    }


    private Vector3 PointProjectOnPlane(Vector3 point, Vector3 origin, Vector3 normal)
    {
        Vector3 op = point - origin;
        float distance = Vector3.Dot(op, normal);

        return point - distance * normal;
    }

    #endregion

    #region Special

    private void TriLAL(float a, float b, float alpha, out float c)
    {
        alpha *= Mathf.Deg2Rad;

        float cosa = Mathf.Cos(alpha);
        float powcosa = cosa * cosa;
        c = Mathf.Sqrt(a * a + b * b - 2 * a * Mathf.Sqrt(b * b * powcosa));
    }

    #endregion

    #region Util

    private void SwapCondition<T>(ref T t1, ref T t2) where T : Condition
    {
        T tmp = t1;
        t1 = t2;
        t2 = tmp;
    }

    private void FormatEdgeRefer(ref EdgeRefer er)
    {
        if (er.id2 == TriPyramid.TOP_S)
        {
            er.id2 = er.id1;
            er.id1 = TriPyramid.TOP_S;
        }
    }

    private void FormatBCornerRefer(ref CornerRefer cr)
    {
        if (cr.id3 == TriPyramid.TOP_S)
        {
            cr.id3 = cr.id1;
            cr.id1 = TriPyramid.TOP_S;
        }
    }

    private bool CanTriangle(float a, float b, float c)
    {
        return (a + b) > c && (a + c) > b && (b + c) > a;
    }

    private bool CanTop(float a, float b, float c, float d, float e, float f)
    {
        // Debug.Log("------- Can Top -------");
        // Debug.Log("a: " + a + "  b: " + b + "  c: " + c);
        bool result = CanTriangle(a, b, c) && CanTriangle(a, d, e) && CanTriangle(b, d, f) && CanTriangle(c, e, f);

        Range<float> rangeA = TriPyramidEdgeRange(a, b, c, d, e);
        result = result && f > rangeA.min && f < rangeA.max;
        // Debug.Log("f: " + f + "  : " + rangeA.min + " - " + rangeA.max);

        Range<float> rangeB = TriPyramidEdgeRange(b, a, c, d, f);
        result = result && e > rangeB.min && e < rangeB.max;
        // Debug.Log("e: " + e + "  : " + rangeB.min + " - " + rangeB.max);

        Range<float> rangeC = TriPyramidEdgeRange(c, a, b, e, f);
        result = result && d > rangeC.min && d < rangeC.max;
        // Debug.Log("d: " + d + "  : " + rangeC.min + " - " + rangeC.max);

        return result;
    }


    // a 共同边    b d 共点    c e 共点
    private Range<float> TriPyramidEdgeRange(float a, float b, float c, float d, float e)
    {
        float h_bc = TriangleTall(b, c, a);
        float h_de = TriangleTall(d, e, a);
        if (b * b < h_bc * h_bc)
            Debug.LogWarning("TriPyramidEdgeRange: l_bc Error    b = " + b + "   h_bc = " + h_bc + " AboutEquals: " + Math.AboutEquals(b * b, h_bc * h_bc));
        if (d * d < h_de * h_de)
            Debug.LogWarning("TriPyramidEdgeRange: l_de Error    d = " + d + "   h_de = " + h_de + " AboutEquals: " + Math.AboutEquals(d * d, h_de * h_de));
        float l_bc = Math.AboutEquals(b * b, h_bc * h_bc) ? 0 : Mathf.Sqrt(b * b - h_bc * h_bc);
        float l_de = Math.AboutEquals(d * d, h_de * h_de) ? 0 : Mathf.Sqrt(d * d - h_de * h_de);
        float l = Mathf.Abs(l_bc - l_de);

        float h_min = Mathf.Abs(h_bc - h_de);
        float h_max = h_bc + h_de;

        float min = Mathf.Sqrt(l * l + h_min * h_min);
        float max = Mathf.Sqrt(l * l + h_max * h_max);

        return new Range<float>(min, max);
    }

    // Calculate h of c
    private float TriangleTall(float a, float b, float c)
    {
        if (CanTriangle(a, b, c))
        {
            float s = (a + b + c) / 2.0f;
            float area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));

            return area * 2.0f / c;
        }

        return 0;
    }

    private float FitLengthOfRange(float min, float max)
    {
        return (min + max) / 2;
    }

    private float EdgeReferLength(EdgeRefer er)
    {
        return EdgeIndexLength(er.id1, er.id2);
    }

    private float EdgeIndexLength(int id1, int id2)
    {
        Vector3 v1 = geometry.UnitVector(id1);
        Vector3 v2 = geometry.UnitVector(id2);
        return Vector3.Magnitude(v1 - v2);
    }

    // return degree
    private float CornerIndexAngle(int id1, int id2, int id3)
    {
        Vector3 v1 = geometry.UnitVector(id1);
        Vector3 v2 = geometry.UnitVector(id2);
        Vector3 v3 = geometry.UnitVector(id3);
        return Mathf.Abs(Vector3.Angle(v1 - v2, v3 - v2));
    }

    private EdgeRefer EdgeOppositeBottom(EdgeRefer er)
    {
        if (er == new EdgeRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_A))
            return new EdgeRefer(TriPyramid.BOTTOM_B, TriPyramid.BOTTOM_C);
        else if (er == new EdgeRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_B))
            return new EdgeRefer(TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_C);
        else if (er == new EdgeRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_C))
            return new EdgeRefer(TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_B);

        Debug.Log("Error EdgeOppositeBottom: " + er);
        return new EdgeRefer(-1, -1);
    }

    private int TCornerOppositeIndex(CornerRefer cr)
    {
        if (cr == new CornerRefer(TriPyramid.BOTTOM_A, TriPyramid.TOP_S, TriPyramid.BOTTOM_B))
            return TriPyramid.BOTTOM_C;
        else if (cr == new CornerRefer(TriPyramid.BOTTOM_A, TriPyramid.TOP_S, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_B;
        else if (cr == new CornerRefer(TriPyramid.BOTTOM_B, TriPyramid.TOP_S, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_A;

        Debug.Log("Error TCornerOppositeIndex: " + cr);
        return -1;
    }

    private int BCornerOppositeIndex(CornerRefer cr)
    {
        if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_B))
            return TriPyramid.BOTTOM_C;
        else if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_B, TriPyramid.BOTTOM_A))
            return TriPyramid.BOTTOM_C;
        else if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_A, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_B;
        else if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_C, TriPyramid.BOTTOM_A))
            return TriPyramid.BOTTOM_B;
        else if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_B, TriPyramid.BOTTOM_C))
            return TriPyramid.BOTTOM_A;
        else if (cr == new CornerRefer(TriPyramid.TOP_S, TriPyramid.BOTTOM_C, TriPyramid.BOTTOM_B))
            return TriPyramid.BOTTOM_A;

        Debug.Log("Error BCornerOppositeIndex: " + cr);
        return -1;
    }

    private int EdgeOppositeIndex(EdgeRefer er)
    {
        if (er == new EdgeRefer(0, 1))
            return 2;
        else if (er == new EdgeRefer(0, 2))
            return 1;
        else if (er == new EdgeRefer(1, 2))
            return 0;

        Debug.Log("Error EdgeOppositeIndex: " + er);
        return -1;
    }

    private int TCornerOtherIndex(CornerRefer cr, int index)
    {
        if (cr.id1 == index)
            return cr.id3;
        else if (cr.id3 == index)
            return cr.id1;

        Debug.Log("Error TCornerOtherIndex: " + cr + " " + index);
        return -1;
    }

    private int TCornersAdjacentIndex(CornerRefer cr1, CornerRefer cr2)
    {
        int[] map = new int[4];
        map[cr1.id1]++;
        map[cr1.id3]++;
        map[cr2.id1]++;
        map[cr2.id3]++;

        if (map[TriPyramid.BOTTOM_A] == 2)
            return TriPyramid.BOTTOM_A;
        else if (map[TriPyramid.BOTTOM_B] == 2)
            return TriPyramid.BOTTOM_B;
        else if (map[TriPyramid.BOTTOM_C] == 2)
            return TriPyramid.BOTTOM_C;

        Debug.Log("Error TCornersAdjacentIndex: " + cr1 + " " + cr2);
        return -1;
    }

    private bool IsEdgeOppositeTCorner(EdgeRefer er, CornerRefer cr)
    {
        int ie;
        if (er.id1 != TriPyramid.TOP_S)
            ie = er.id1;
        else if (er.id2 != TriPyramid.TOP_S)
            ie = er.id2;
        else
        {
            Debug.Log("Error IsEdgeOppositeTCorner: " + er + " " + cr);
            return false;
        }

        return ie != cr.id1 && ie != cr.id3;
    }

    private bool IsEdgeAdjacentTCorner(EdgeRefer er, CornerRefer cr)
    {
        int ie;
        if (er.id1 != TriPyramid.TOP_S)
            ie = er.id1;
        else if (er.id2 != TriPyramid.TOP_S)
            ie = er.id2;
        else
        {
            Debug.Log("Error IsEdgeAdjacentTCorner: " + er + " " + cr);
            return false;
        }

        return ie == cr.id1 || ie == cr.id3;
    }

    private bool IsEdgeForwardBCorner(EdgeRefer er, CornerRefer cr)
    {
        int ie;
        if (er.id1 != TriPyramid.TOP_S)
            ie = er.id1;
        else if (er.id2 != TriPyramid.TOP_S)
            ie = er.id2;
        else
        {
            Debug.Log("Error IsEdgeForwardBCorner: " + er + " " + cr);
            return false;
        }

        return ie == cr.id2;
    }

    private bool IsEdgeBackwardBCorner(EdgeRefer er, CornerRefer cr)
    {
        int ic, ie;
        if (cr.id1 != TriPyramid.TOP_S)
            ic = cr.id1;
        else if (cr.id3 != TriPyramid.TOP_S)
            ic = cr.id3;
        else
        {
            Debug.Log("Error IsEdgeBackwardBCorner: " + er + " " + cr);
            return false;
        }
        if (er.id1 != TriPyramid.TOP_S)
            ie = er.id1;
        else if (er.id2 != TriPyramid.TOP_S)
            ie = er.id2;
        else
        {
            Debug.Log("Error IsEdgeBackwardBCorner: " + er + " " + cr);
            return false;
        }

        return ic == ie;
    }

    private bool IsEdgeOppositeBCorner(EdgeRefer er, CornerRefer cr)
    {
        int ic, ie;
        if (cr.id1 != TriPyramid.TOP_S)
            ic = cr.id1;
        else if (cr.id3 != TriPyramid.TOP_S)
            ic = cr.id3;
        else
        {
            Debug.Log("Error IsEdgeOppositeBCorner: " + er + " " + cr);
            return false;
        }
        if (er.id1 != TriPyramid.TOP_S)
            ie = er.id1;
        else if (er.id2 != TriPyramid.TOP_S)
            ie = er.id2;
        else
        {
            Debug.Log("Error IsEdgeOppositeBCorner: " + er + " " + cr);
            return false;
        }

        return ie != ic && ie != cr.id2;
    }

    // X
    private bool IsBCornersContrary(CornerRefer cr1, CornerRefer cr2)
    {
        int id1, id2;
        if (cr1.id1 != TriPyramid.TOP_S)
            id1 = cr1.id1;
        else if (cr1.id3 != TriPyramid.TOP_S)
            id1 = cr1.id3;
        else
        {
            Debug.Log("Error IsBCornersContrary: " + cr1 + " " + cr2);
            return false;
        }
        if (cr2.id1 != TriPyramid.TOP_S)
            id2 = cr2.id1;
        else if (cr2.id3 != TriPyramid.TOP_S)
            id2 = cr2.id3;
        else
        {
            Debug.Log("Error IsBCornersContrary: " + cr1 + " " + cr2);
            return false;
        }

        return cr1.id2 != cr2.id2 && id1 == id2;
    }

    // Y 
    private bool IsBCornersRelative(CornerRefer cr1, CornerRefer cr2)
    {
        int id1, id2;
        if (cr1.id1 != TriPyramid.TOP_S)
            id1 = cr1.id1;
        else if (cr1.id3 != TriPyramid.TOP_S)
            id1 = cr1.id3;
        else
        {
            Debug.Log("Error IsBCornersRelative: " + cr1 + " " + cr2);
            return false;
        }
        if (cr2.id1 != TriPyramid.TOP_S)
            id2 = cr2.id1;
        else if (cr2.id3 != TriPyramid.TOP_S)
            id2 = cr2.id3;
        else
        {
            Debug.Log("Error IsBCornersRelative: " + cr1 + " " + cr2);
            return false;
        }

        return cr1.id2 == cr2.id2 && id1 != id2;
    }

    // Z
    private bool IsBCornersParallel(CornerRefer cr1, CornerRefer cr2)
    {
        int id1, id2;
        if (cr1.id1 != TriPyramid.TOP_S)
            id1 = cr1.id1;
        else if (cr1.id3 != TriPyramid.TOP_S)
            id1 = cr1.id3;
        else
        {
            Debug.Log("Error IsBCornersParallel: " + cr1 + " " + cr2);
            return false;
        }
        if (cr2.id1 != TriPyramid.TOP_S)
            id2 = cr2.id1;
        else if (cr2.id3 != TriPyramid.TOP_S)
            id2 = cr2.id3;
        else
        {
            Debug.Log("Error IsBCornersParallel: " + cr1 + " " + cr2);
            return false;
        }

        return id1 == cr2.id2 && id2 != cr1.id2;
    }

    private bool IsTCornerStaggerBCorner(CornerRefer tcr, CornerRefer bcr)
    {
        int ib;
        if (bcr.id1 != TriPyramid.TOP_S)
            ib = bcr.id1;
        else if (bcr.id3 != TriPyramid.TOP_S)
            ib = bcr.id3;
        else
        {
            Debug.Log("Error IsTCornerStaggerBCorner: " + tcr + " " + bcr);
            return false;
        }

        return !(ib == tcr.id1 && bcr.id2 == tcr.id3) && !(ib == tcr.id3 && bcr.id2 == tcr.id1);
    }

    private bool IsBCornerOppositeTCorner(CornerRefer bcr, CornerRefer tcr)
    {
        int ib;
        if (bcr.id1 != TriPyramid.TOP_S)
            ib = bcr.id1;
        else if (bcr.id3 != TriPyramid.TOP_S)
            ib = bcr.id3;
        else
        {
            Debug.Log("Error IsBCornerOppositeTCorner: " + tcr + " " + bcr);
            return false;
        }


        return ib == tcr.id1 || ib == tcr.id3;
    }

    private bool IsBCornerAdjacentTCorner(CornerRefer bcr, CornerRefer tcr)
    {
        return bcr.id2 == tcr.id1 || bcr.id2 == tcr.id3;
    }

    private bool Assert(bool condition, string message)
    {
        if (!condition)
            Debug.LogWarning("Error " + message);
        return condition;
    }

    #endregion
}
