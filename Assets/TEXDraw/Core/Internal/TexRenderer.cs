using UnityEngine;

namespace TexDrawLib
{
    public class TexRenderer : IFlushable
    {
        public static TexRenderer Get(HorizontalBox box, TexMetaRenderer meta, int isPartOfPrev = 0)
        {
            var renderer = ObjPool<TexRenderer>.Get();
            renderer.Box = box;
            //renderer.Scale = scale;
            renderer.metaRules = meta;
            renderer.partOfPreviousLine = isPartOfPrev;

            // must horizontal box to avoid headache
            if (!(box is HorizontalBox))
                renderer.Box = box = HorizontalBox.Get(box);

            // There's an additional step if meta 'line' are declared
            if (renderer.usingMetaRules && renderer.metaRules.line != 0 && box.totalHeight > 0)
            {
                box.height = renderer.metaRules.line;
                box.depth = TexUtility.spaceLine;
            }
            return renderer;
        }

        public HorizontalBox Box;

        /// Used for internal param rendering. 0 = No (Meaning it's first line of paragraph), 1 = Yes, 2 = Yes (and there's a space erased)
		public int partOfPreviousLine = 0;

        public TexMetaRenderer metaRules;

        public bool usingMetaRules { get { return metaRules != null && metaRules.enabled; } }

        /// This Penaltied amounts is depends on metaRules

        public float Width { get { return Mathf.Max(Box.width, 0); } }

        public float Height { get { return Mathf.Max(Box.height, TexUtility.spaceHeight); } }

        public float Depth { get { return Mathf.Max(Box.depth, TexUtility.spaceDepth); } }

        public float PenaltyWidth { get { return usingMetaRules ? (metaRules.left + metaRules.right + (partOfPreviousLine == 0 ? metaRules.leading : 0)) : 0; } }

        public float PenaltySpacing { get { return TEXConfiguration.main.LineSpace + (usingMetaRules ? metaRules.spacing : 0); } }

        public float PenaltyParagraph { get { return usingMetaRules && partOfPreviousLine == 0 ? metaRules.paragraph : 0; } }

        public float CompleteHeight { get { return Height + Depth + PenaltySpacing + PenaltyParagraph; } }

        public bool IsMetaBlock { get { return Box.children.Count == 1 && Box.children[0] is StrutBox && ((StrutBox)Box.children[0]).policy == StrutPolicy.MetaBlock; } }

        public void Render(DrawingContext drawingContext, float x, float y, float Scale)
        {
            if (Box != null)
                Box.Draw(drawingContext, Scale, X = x / Scale, Y = y / Scale);
        }

        public void Flush()
        {
            if (Box != null)
            {
                Box.Flush();
                Box = null;
            }
            partOfPreviousLine = 0;
            if (metaRules != null)
            {
                //metaRules.Flush();
                metaRules = null;
            }
            ObjPool<TexRenderer>.Release(this);
        }

        internal float X, Y;

        private bool m_flushed = false;
        public bool IsFlushed { get { return m_flushed; } set { m_flushed = value; } }
    }
}
