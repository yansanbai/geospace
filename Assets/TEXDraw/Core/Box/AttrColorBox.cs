using UnityEngine;

namespace TexDrawLib
{
    public class AttrColorBox : Box
    {
        public static AttrColorBox Get(int mixMode, Color color)
        {
            var box = ObjPool<AttrColorBox>.Get();
            box.color = color;
            box.mixmode = mixMode;
            // leave the size zero
            return box;
        }

        //        public AttrColorAtom attachedAtom;
        public Color color;

        //      public Color endColor;
        //If null, then this is the end box
        //    public AttrColorBox endBox;

        /// <summary>
        /// 0 = Overwrite, 1 = Alpha-Multiply, 2 = RGBA-Multiply, 3 = This is popping command
        /// </summary>
        public int mixmode;

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            switch (mixmode)
            {
                case 0: TexContext.Color.Push(color); break;
                case 1: TexContext.Color.Push(TexUtility.MultiplyAlphaOnly(color, TexContext.Color.value.a / 255f)); break;
                case 2: TexContext.Color.Push(TexUtility.MultiplyColor(TexContext.Color.value, color)); break;
                case 3: TexContext.Color.Pop(); break;
            }
        }

        //void ProcessFinalColor(Color32 old)
        //{
        //    switch (mixMode) {
        //        case 1:
        //            return TexUtility.MultiplyAlphaOnly(color, old.a / 255f);
        //        case 2:
        //            return TexUtility.MultiplyColor(old, color);
        //    }
        //    return color;
        //}

        public override void Flush()
        {
            ObjPool<AttrColorBox>.Release(this);
        }
    }
}
