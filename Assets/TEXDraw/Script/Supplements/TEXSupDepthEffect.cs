using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Depth Effect", 16), ExecuteInEditMode]
    [TEXSupHelpTip("Create a sense of depth by duplicating faces for given iteration times")]
    public class TEXSupDepthEffect : TEXDrawMeshEffectBase
    {
        [Range(0, 20)]
        public int m_Samples = 0;

        [Range(0, 5)]
        public float m_DepthDistance = .5f;

        public bool m_DrawBackFace = true;
        public bool m_Positive = false;

        [Range(0, 1)]
        public float m_BodyAlpha = 1;

        public Color m_Color = Color.black;

        [System.NonSerialized]
        private VertexHelper vh = new VertexHelper();

        private static List<UIVertex> stream = new List<UIVertex>();

        public override void ModifyMesh(Mesh m)
        {
            vh.Clear();
            stream.Clear();
            new VertexHelper(m).GetUIVertexStream(stream);
            vh.AddUIVertexTriangleStream(stream);
            int start = 0, count = 0;
            float depth = m_DepthDistance / m_Samples;
            for (int i = m_Samples; i-- > 0;)
            {
                start = i == m_Samples - 2 ? 0 : count;
                count = vh.currentVertCount;
                if (count + (count - start) >= 65000)
                    break;
                ApplyShadowZeroAlloc(vh, m_Color, start, count, depth * (m_Positive ? -1 : 1) * (i + 1), i == m_Samples - 1);
            }
            vh.AddUIVertexTriangleStream(stream);
            vh.FillMesh(m);
        }

        protected void ApplyShadowZeroAlloc(VertexHelper verts, Color32 color, int start, int end, float z, bool justReplace)
        {
            UIVertex uIVertex = new UIVertex();

            for (int i = start; i < end; i++)
            {
                verts.PopulateUIVertex(ref uIVertex, i);
                Vector3 position = uIVertex.position;
                position.z = z;
                uIVertex.position = position;
                uIVertex.color = Color.Lerp(uIVertex.color, color, m_BodyAlpha);
                if (justReplace)
                {
                    verts.SetUIVertex(uIVertex, i);
                }
                else
                {
                    verts.AddVert(uIVertex);
                    if (i % 4 == 0)
                    {
                        var t = verts.currentVertCount - 1;
                        verts.AddTriangle(t + 0, t + 1, t + 3);
                        verts.AddTriangle(t + 3, t + 1, t + 2);
                    }
                }
            }
        }
    }
}
