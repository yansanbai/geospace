using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Warp By Transform")]
    [TEXSupHelpTip("Make characters have a warped effect")]
    public class TEXSupWarpByTransform : TEXDrawMeshEffectBase
    {
        public ScaleOffset factor = ScaleOffset.identity;
        public AnimationCurve topBend = AnimationCurve.Linear(0, 0, 1, 1);
        public ScaleOffset topFactor = ScaleOffset.identity;
        public ScaleOffset topSlant = ScaleOffset.identity;
        public AnimationCurve bottomBend = AnimationCurve.Linear(0, 0, 1, 1);
        public ScaleOffset bottomFactor = ScaleOffset.identity;
        public ScaleOffset bottomSlant = ScaleOffset.identity;

        [Range(0, 1)]
        public float slantAlignment = .5f;

        public override void ModifyMesh(Mesh m)
        {
            var verts = ListPool<Vector3>.Get();
            m.GetVertices(verts);
            var count = verts.Count;

            b_left = m.bounds.min.x;
            b_right = m.bounds.max.x;
            b_scale = tex.drawingParams.scale;
            for (int i = 0; i < count;)
            {
                verts[i] = EvaluateSlant(verts[i], false); i++;
                verts[i] = EvaluateSlant(verts[i], false); i++;
                verts[i] = EvaluateSlant(verts[i], true); i++;
                verts[i] = EvaluateSlant(verts[i], true); i++;
            }

            m.SetVertices(verts);
            ListPool<Vector3>.Release(verts);
        }

        private float b_left;
        private float b_right;
        private float b_scale;

        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return /*Mathf.Clamp01*/((value - a) / (b - a));
            }
            return 0;
        }

        private float EvaluateUp(float x, bool isTop)
        {
            x = factor.Evaluate(x);
            if (isTop)
                return topFactor.Evaluate(topBend.Evaluate(InverseLerp(b_left, b_right, x))) * b_scale;
            else
                return bottomFactor.Evaluate(bottomBend.Evaluate(InverseLerp(b_left, b_right, x))) * b_scale;
        }

        private Vector3 EvaluateSlant(Vector3 v, bool isTop)
        {
            float xInterp = InverseLerp(b_left, b_right, v.x);
            float xTarget = (isTop ? topSlant : bottomSlant).Evaluate(xInterp - slantAlignment) + slantAlignment;
            v.x = Mathf.LerpUnclamped(b_left, b_right, xTarget);
            v.y += EvaluateUp(v.x, isTop);
            return v;
        }

        /*public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
		public float multiplier = 1;
		//public bool useAdvancedFeatures;
		public float rotateAmount = 0f;
		//public float realignAmount = 0f;
		const string repFormat = @"\trs[TY{0}]{{{1}}}";

		protected override string Subtitute(string match, float factor)
		{
			//TODO: Do string concat is faster than format?

			if(float.IsNaN(factor))
				factor = 0;
			if (rotateAmount == 0) {
			return @"\trs[TY" + (curve.Evaluate(factor) * multiplier).ToString()
				+ @"]{" + match + "}";
			}
			var now = curve.Evaluate(factor);
			var speed = now - curve.Evaluate(factor < 0.5f ? factor + 0.01f : factor - 0.01f);
			speed *= factor < 0.5f ? -multiplier : multiplier;
			speed *= rotateAmount;
			speed = Mathf.Atan(speed);

			//var offset = (curve.Evaluate(factor + perItemExtent) + curve.Evaluate(factor - perItemExtent)) / 2f - now;
			//if (factor < .001f || factor > .999f)
			//	offset = 0;
			return @"\trs[T0" + (-offset * realignAmount).ToString("0.000")+  "," + (curve.Evaluate(factor + Mathf.Abs(offset)*realignAmount) * multiplier).ToString("0.000")
				+ "R" + (speed * Mathf.Rad2Deg).ToString() + @"]{" + match + "}";
		}

		//float perItemExtent;
		protected override void OnBeforeSubtitution(float count)
		{
			//perItemExtent = 1f / count;
		}*/
    }
}
