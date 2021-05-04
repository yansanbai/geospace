using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Vertex Gradient", 16), ExecuteInEditMode]
    [TEXSupHelpTip("Blend vertex colors on each vertex corner")]
    public class TEXSupVertexGradient : TEXDrawMeshEffectBase
    {
        public Color topLeft = Color.white;
        public Color topRight = Color.white;
        public Color bottomRight = Color.white;
        public Color bottomLeft = Color.white;

        public override void ModifyMesh(Mesh m)
        {
            var colors = ListPool<Color32>.Get();
            m.GetColors(colors);
            var count = colors.Count;

            for (int i = 0; i < count;)
            {
                colors[i++] *= bottomLeft;
                colors[i++] *= bottomRight;
                colors[i++] *= topRight;
                colors[i++] *= topLeft;
            }

            m.SetColors(colors);
            ListPool<Color32>.Release(colors);
        }
    }
}
