using System.Collections.Generic;
using UnityEngine;

namespace TexDrawLib
{
    public class AttrTransformationBox : Box
    {
        public static AttrTransformationBox Get(AttrTransformationAtom atom, AttrTransformationBox endBox)
        {
            var box = ObjPool<AttrTransformationBox>.Get();
            box.pivotMode = atom.pivotMode;
            box.endBox = endBox;
            box.attachedAtom = atom;
            return box;
        }

        public AttrTransformationAtom attachedAtom;

        //If null, then this is the end box
        public AttrTransformationBox endBox;

        // 0 = Invidual, 1 = Median, 2 = Local
        public int pivotMode;

        public int endLimit;

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            //AttrTransformationAtom.ScaleTransformation(ref _m, Vector3.one * scale);
            if (endBox != null)
                endBox.endLimit = drawingContext.vertex.vertexcount;
            else
            {
                if (!attachedAtom.dualMatrix)
                    DoSingleTransformation(drawingContext);
            }
        }

        private void DoSingleTransformation(DrawingContext drawingContext)
        {
            var _m = attachedAtom.matrix;
            var v = drawingContext.vertex.m_Positions;
            var vC = drawingContext.vertex.vertexcount;
            var offset = Vector3.zero;
            if (pivotMode == 1)
                offset = FindMedian(v, endLimit, vC - endLimit);
            for (int i = endLimit; i < vC;)
            {
                if (pivotMode == 0)
                    offset = FindMedian(v, i);

                v[i] = _m.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _m.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _m.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _m.MultiplyPoint3x4(v[i++] - offset) + offset;
            }
        }

        private void DoDoubleTransformation(DrawingContext drawingContext)
        {
            var _mL = attachedAtom.matrix;
            var _mR = attachedAtom.secondMatrix;
            var v = drawingContext.vertex.m_Positions;
            var vC = drawingContext.vertex.vertexcount;
            var offset = Vector3.zero;
            if (pivotMode == 1)
                offset = FindMedian(v, endLimit, vC - endLimit);
            for (int i = endLimit; i < vC;)
            {
                if (pivotMode == 0)
                    offset = FindMedian(v, i);

                v[i] = _mL.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _mR.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _mR.MultiplyPoint3x4(v[i++] - offset) + offset;
                v[i] = _mL.MultiplyPoint3x4(v[i++] - offset) + offset;
            }
        }

        private static Vector3 FindMedian(List<Vector3> list, int idx, int count = 4)
        {
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = count; i-- > 0;)
            {
                var v = list[i + idx];
                min = Vector3.Min(v, min);
                max = Vector3.Max(v, max);
            }
            return Vector3.LerpUnclamped(min, max, 0.5f);
        }

        public override void Flush()
        {
            endBox = null;
            endLimit = 0;
            attachedAtom.generatedBox = null;
            attachedAtom = null;
            ObjPool<AttrTransformationBox>.Release(this);
        }
    }
}
