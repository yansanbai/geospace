namespace TexDrawLib
{
    // Represents graphical box that is part of math expression, and can itself contain child boxes.
    public abstract class Box : IFlushable
    {
        protected Box() { }

        public float totalHeight { get { return height + depth; } }

        public float width, height, depth, shift;

        public void Set(float w, float h, float d, float s)
        {
            width = w;
            height = h;
            depth = d;
            shift = s;
        }

        public virtual void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
#if TEXDRAW_DEBUG
            // Cool debugging feature
            if (TEXConfiguration.main.Debug_HighlightBoxes)
                drawingContext.DrawWireDebug(new UnityEngine.Rect(x * scale, (y - depth) * scale, width * scale, totalHeight * scale), new UnityEngine.Color(1, 1, 0, 0.07f));
#endif
        }

        public abstract void Flush();

        public bool IsFlushed { get; set; }

        public override string ToString()
        {
            return base.ToString().Replace("TexDrawLib.", string.Empty) +
                string.Format(" H:{0:F2} D:{1:F2} W:{2:F2} S:{3:F2}", height, depth, width, shift);
        }
    }
}
