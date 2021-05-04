using UnityEngine;

namespace TexDrawLib
{
    public class AttrTransformationAtom : Atom
    {
        public static AttrTransformationAtom Get(string transformStr, int pivotMode, out AttrTransformationAtom endBlock)
        {
            var atom = ObjPool<AttrTransformationAtom>.Get();
            endBlock = ObjPool<AttrTransformationAtom>.Get();
            atom.EndAtom = endBlock;
            atom.pivotMode = pivotMode;
            var dualIndicator = transformStr.IndexOf('|');
            atom.dualMatrix = dualIndicator >= 0;
            atom.matrix = ParseTransformation(transformStr);
            if (atom.dualMatrix)
            {
                atom.matrix = ParseTransformation(transformStr.Substring(0, dualIndicator));
                atom.secondMatrix = ParseTransformation(transformStr.Substring(dualIndicator + 1));
            }
            else
                atom.matrix = ParseTransformation(transformStr);

            endBlock.matrix = atom.matrix;
            endBlock.pivotMode = atom.pivotMode;
            return atom;
        }

        public AttrTransformationAtom EndAtom;

        public AttrTransformationBox generatedBox;

        public Matrix4x4 matrix = _identity;
        public Matrix4x4 secondMatrix = _identity;
        public int pivotMode;
        public bool dualMatrix;

        public override Box CreateBox()
        {
            if (generatedBox != null)
            {
                return generatedBox;
            }

            generatedBox = AttrTransformationBox.Get(this,
                EndAtom == null ? null : (AttrTransformationBox)EndAtom.CreateBox());
            //ScaleTransformation(ref generatedBox.matrix, Vector3.one * TexContext.Scale);
            return generatedBox;
        }

        public override void Flush()
        {
            EndAtom = null;
            matrix = _identity;
            generatedBox = null;
            ObjPool<AttrTransformationAtom>.Release(this);
        }

        //----------------------------------------------------------
        // Some crazy Matrix4x4 additional operations go here ...

        private static readonly Matrix4x4 _identity = Matrix4x4.identity;

        static public bool IsCharADigit(char c, bool withBegin = true)
        {
            return (char.IsDigit(c) || c == '.' || (withBegin && (c == '-' || c == '/')));
        }

        private const string _TParseIdentity = "XYZ";

        static public float ExtractDigit(string value, ref int pos, float def = 0f)
        {
            // if empty ...
            if (pos == value.Length)
                return def;
            // if this is a param ...
            if (value[pos] == '(')
            {
                while (pos < value.Length && (value[pos] != ')'))
                    pos++;
                // pos still on ')', so ...
                pos++;
                return float.NaN;
            }
            // if we want a divisor (1/n stuff) ...
            bool divMode = value[pos] == '/';
            if (divMode)
                pos++;
            if (pos == value.Length)
                return def;
            // if this is a negative number ...
            bool minMode = value[pos] == '-';
            if (minMode)
                pos++;
            int start = pos;
            // find where number ends ...
            while (pos < value.Length)
            {
                var c = value[pos];
                if (!IsCharADigit(c, false))
                    break;
                else
                    pos++;
            }
            // ... and parse it ...
            float result;
            if (!float.TryParse(value.Substring(start, pos - start), out result))
                result = def;
            // ... and don't forget the rules ...
            if (divMode && result != 0f)
                result = 1f / result;
            if (minMode)
                result *= -1f;
            // finally ...
            return result;
        }

        public static Matrix4x4 ParseTransformation(string value)
        {
            var original = _identity;
            if (string.IsNullOrEmpty(value))
                return original;
            // Regards to position
            var pos = 0;
            while (pos < value.Length)
            {
                var c = value[pos];
                pos++;
                if (c == 'S')
                    AppendScale(ref original, SubParseTransformation(ref pos, value, 2));
                else if (c == 'R')
                    AppendRotation(ref original, Quaternion.Euler(SubParseTransformation(ref pos, value, 1)));
                else if (c == 'T')
                    AppendTranslation(ref original, SubParseTransformation(ref pos, value, 0));
                else if (IsCharADigit(c) || c == '(' || _TParseIdentity.IndexOf(c) >= 0)
                {
                    pos--;
                    AppendTranslation(ref original, (SubParseTransformation(ref pos, value, 0)));
                }
                else
                    break;
            }
            return original;
        }

        private static Vector3 SubParseTransformation(ref int pos, string value, int mode)
        {
            var n = mode == 2 ? 1f : 0f;
            if (pos >= value.Length)
                return new Vector3(n, n, n);
            var c = value[pos];
            if (IsCharADigit(c) || c == '(')
            {
                float x = ExtractDigit(value, ref pos);
                if (pos < value.Length && value[pos] == ',')
                {
                    pos++;
                    float y = ExtractDigit(value, ref pos);
                    if (pos < value.Length && value[pos] == ',')
                    {
                        pos++;
                        float z = ExtractDigit(value, ref pos);
                        return new Vector3(x, y, z);
                    }
                    else
                        return new Vector3(x, y, n);
                }
                else
                    return mode == 2 ? new Vector3(x, x, x) : new Vector3(n, n, x);
            }
            else
            {
                var idx = _TParseIdentity.IndexOf(c); // lrudfb
                if (idx >= 0)
                {
                    pos++;
                    float v = ExtractDigit(value, ref pos);
                    return GetVectorParsingCode(idx, v, mode == 2);
                }
            }
            return new Vector3(n, n, n);
        }

        private static Vector3 GetVectorParsingCode(int code, float size, bool scale)
        {
            var n = scale ? 1f : 0f;
            switch (code)
            {
                //case 0:
                //	return new Vector3 (-size, n, n);
                case 0:
                    return new Vector3(size, n, n);
                //case 2:
                //	return new Vector3 (n, -size, n);
                case 1:
                    return new Vector3(n, size, n);
                //case 4:
                //	return new Vector3 (n, n, -size);
                case 2:
                    return new Vector3(n, n, size);
            }
            return Vector3.zero;
        }

        static public void AppendTranslation(ref Matrix4x4 m, Vector3 pos)
        {
            // TR;LD: m * TRS(pos, 0, 0)
            float x = pos.x, y = pos.y, z = pos.z;
            m.m03 = m.m00 * x + m.m01 * y + m.m02 * z + m.m03;
            m.m13 = m.m10 * x + m.m11 * y + m.m12 * z + m.m13;
            m.m23 = m.m20 * x + m.m21 * y + m.m22 * z + m.m23;
            m.m33 = m.m30 * x + m.m31 * y + m.m32 * z + m.m33;
        }

        static public void AppendRotation(ref Matrix4x4 m, Quaternion rot)
        {
            m = m * Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);

            /*float x2 = rot.x + rot.x;
			float y2 = rot.y + rot.y;
			float z2 = rot.z + rot.z;

			float wx2 = rot.w * x2;
			float wy2 = rot.w * y2;
			float wz2 = rot.w * z2;
			float xx2 = rot.x * x2;
			float xy2 = rot.x * y2;
			float xz2 = rot.x * z2;
			float yy2 = rot.y * y2;
			float yz2 = rot.y * z2;
			float zz2 = rot.y * z2;

			float q11 = 1.0f - yy2 - zz2;
			float q21 = xy2 - wz2;
			float q31 = xz2 + wy2;

			float q12 = xy2 + wz2;
			float q22 = 1.0f - xx2 - zz2;
			float q32 = yz2 - wx2;

			float q13 = xz2 - wy2;
			float q23 = yz2 + wx2;
			float q33 = 1.0f - xx2 - yy2;

			Matrix4x4 result;

			// First row
			result.m00 = m.m00 * q11 + m.m01 * q21 + m.m02 * q31;
			result.m01 = m.m00 * q12 + m.m01 * q22 + m.m02 * q32;
			result.m02 = m.m00 * q13 + m.m01 * q23 + m.m02 * q33;
			result.m03 = m.m03;

			// Second row
			result.m10 = m.m10 * q11 + m.m11 * q21 + m.m12 * q31;
			result.m11 = m.m10 * q12 + m.m11 * q22 + m.m12 * q32;
			result.m12 = m.m10 * q13 + m.m11 * q23 + m.m12 * q33;
			result.m13 = m.m13;

			// Third row
			result.m20 = m.m20 * q11 + m.m21 * q21 + m.m22 * q31;
			result.m21 = m.m20 * q12 + m.m21 * q22 + m.m22 * q32;
			result.m22 = m.m20 * q13 + m.m21 * q23 + m.m22 * q33;
			result.m23 = m.m23;

			// Fourth row
			result.m30 = m.m30 * q11 + m.m31 * q21 + m.m32 * q31;
			result.m31 = m.m30 * q12 + m.m31 * q22 + m.m32 * q32;
			result.m32 = m.m30 * q13 + m.m31 * q23 + m.m32 * q33;
			result.m33 = m.m33;

			m = result;*/
        }

        static public void AppendScale(ref Matrix4x4 m, Vector3 scl)
        {
            // TR;LD: m * TRS(0, 0, rot)
            float x = scl.x, y = scl.y, z = scl.z;
            m.m00 *= x;
            m.m01 *= y;
            m.m02 *= z;
            m.m10 *= x;
            m.m11 *= y;
            m.m12 *= z;
            m.m20 *= x;
            m.m21 *= y;
            m.m22 *= z;
            m.m30 *= x;
            m.m31 *= y;
            m.m32 *= z;
        }

        static public void ScaleTransformation(ref Matrix4x4 m, Vector3 scl)
        {
            // TR;LD: m * TRS(0, 0, rot)
            float x = scl.x, y = scl.y, z = scl.z;
            m.m03 *= x;
            m.m13 *= y;
            m.m23 *= z;
        }
    }
}
