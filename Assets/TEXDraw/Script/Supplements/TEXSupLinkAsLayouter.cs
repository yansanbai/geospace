using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Link As Layouter", 16), ExecuteInEditMode]
    [TEXSupHelpTip("Use \\link command to modify given UI objects position")]
    public class TEXSupLinkAsLayouter : TEXDrawMeshEffectBase
    {
        [Tooltip("Filter used link with given keyword, or no at all")]
        public string tagFilter = "";

        [Tooltip("Set target UI transform. Order by first mentioned \\link commmand")]
        public RectTransform[] layouts;

        [System.NonSerialized]
        private bool dirty = false;

        public override void ModifyMesh(Mesh m)
        {
            SetDirty();
        }

        public void SetDirty()
        {
            dirty = true;
            // Can't update now because rebuild-inside-of-rebuild-and-can't-be-interrupred-stuff
            // Do in LateUpdate instead
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorApplication.update += EditorUpdate;
#endif
        }

#if UNITY_EDITOR

        private void EditorUpdate()
        {
            LateUpdate();
            EditorApplication.update -= EditorUpdate;
        }

#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            dirty = true;
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                SetDirty();
                transform.hasChanged = false;
            }
        }

        private void LateUpdate()
        {
            if (!dirty)
                return;
            dirty = false;
            var r = GetComponent<RectTransform>();
            var keys = tex.drawingContext.linkBoxKey;
            var rects = tex.drawingContext.linkBoxRect;

            if (!r || keys.Count == 0 || layouts.Length == 0)
                return;

            var iter = 0;
            var scale = (Vector2)transform.lossyScale;
            scale = new Vector2(1 / scale.x, 1 / scale.y);
            for (int i = 0; i < keys.Count; i++)
            {
                if (string.IsNullOrEmpty(tagFilter) || keys[i].Contains(tagFilter))
                {
                    var layout = layouts[iter];
                    if (layout)
                    {
                        var rect = GetWorldRect(r, rects[iter]);
                        layout.sizeDelta = Vector2.Scale(rect.size, scale);
                        layout.position = new Vector2(rect.x + rect.width * layout.pivot.x, rect.y + rect.height * layout.pivot.y);
                    }
                    if (++iter >= layouts.Length)
                        break;
                }
            }
        }

        private static Rect GetWorldRect(Transform r, Rect rr)
        {
            var pos = r.TransformPoint(rr.position);
            return new Rect(pos, r.TransformPoint(rr.max) - pos);
        }

        private static Rect GetLocalRect(Transform r, Rect rr)
        {
            var pos = r.InverseTransformPoint(rr.position);
            return new Rect(pos, r.InverseTransformPoint(rr.max) - pos);
        }
    }
}
