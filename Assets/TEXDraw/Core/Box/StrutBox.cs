namespace TexDrawLib
{
    // Box representing whitespace.
    public class StrutBox : Box
    {
        //private static readonly StrutBox emptyStrutBox = StrutBox.Get (0, 0, 0, 0);

        public static StrutBox Empty { get { return Get(0, 0, 0, 0); } }

        public static StrutBox EmptyMeta { get { return Get(0, 0, 0, 0, StrutPolicy.MetaBlock); } }

        public static StrutBox EmptyLine { get { return Get(0, TexUtility.spaceHeight, TexUtility.spaceDepth, 0, StrutPolicy.EmptyLine); } }

        public static StrutBox Get(float Width, float Height, float Depth, float Shift)
        {
            var box = ObjPool<StrutBox>.Get();
            box.Set(Width, Height, Depth, Shift);
            box.policy = StrutPolicy.Misc;
            return box;
        }

        public static StrutBox Get(float Width, float Height, float Depth, float Shift, StrutPolicy Policy)
        {
            var box = ObjPool<StrutBox>.Get();
            box.Set(Width, Height, Depth, Shift);
            box.policy = Policy;
            return box;
        }

        public static StrutBox GetBlankSpace()
        {
            return Get(TexUtility.spaceWidth, TexUtility.spaceHeight, TexUtility.spaceDepth, 0, StrutPolicy.BlankSpace);
        }

        public StrutPolicy policy;

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);
        }

        public override void Flush()
        {
            ObjPool<StrutBox>.Release(this);
        }
    }
}
