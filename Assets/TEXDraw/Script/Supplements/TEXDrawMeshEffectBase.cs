using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Fix TMP", 16), ExecuteInEditMode]
    public abstract class TEXDrawMeshEffectBase : BaseMeshEffect
    {
        private MonoBehaviour m_target;

        /// Fast utility to get m_targeted TEXDraw component
        public ITEXDraw tex { get { return (ITEXDraw)m_target; } }

        protected override void OnEnable()
        {
            List<MonoBehaviour> l = ListPool<MonoBehaviour>.Get();
            base.OnEnable();
            GetComponents(l);
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i] is ITEXDraw)
                {
                    m_target = l[i];
                    tex.SetSupplementDirty();
                    ListPool<MonoBehaviour>.ReleaseNoFlush(l);
                    return;
                }
            }
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            if (m_target)
                tex.SetTextDirty(true);
        }

#endif

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_target)
                tex.SetSupplementDirty();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_target)
                tex.SetSupplementDirty();
        }

#if !(UNITY_5_2_1 || UNITY_5_2_2)

        /// <summary>
        /// Don't override this method! Use ModifyMesh(Mesh) instead
        /// </summary>
        public override void ModifyMesh(VertexHelper h)
        {
        }

#endif
    }
}
