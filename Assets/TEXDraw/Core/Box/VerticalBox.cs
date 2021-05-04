using System.Collections.Generic;

using UnityEngine;

namespace TexDrawLib
{
    // Box containing vertical stack of child boxes.
    public class VerticalBox : Box
    {
        private float leftMostPos = float.MaxValue;
        private float rightMostPos = float.MinValue;
        public bool ExtensionMode = false;

        public List<Box> children = new List<Box>();

        public static VerticalBox Get(Box Box, float Height, TexAlignment Alignment)
        {
            var box = Get();
            if (Box.totalHeight >= Height)
            {
                box.Add(Box);
                return box;
            }
            float rest = Height - Box.totalHeight;
            if (Alignment == TexAlignment.Center)
            {
                var strutBox = StrutBox.Get(0, rest * 0.5f, 0, 0);
                box.Add(strutBox);
                box.Add(Box);
                box.Add(strutBox);
                box.Shift(Box.height);
            }
            else if (Alignment == TexAlignment.Top)
            {
                box.Add(Box);
                box.Add(StrutBox.Get(0, rest, 0, 0));
            }
            else if (Alignment == TexAlignment.Bottom)
            {
                box.Add(StrutBox.Get(0, rest, 0, 0));
                box.Add(Box);
                box.Shift(-rest + Box.height);
            }
            return box;
        }

        public static VerticalBox Get(Box Box)
        {
            var box = Get();
            box.Add(Box);
            return box;
        }

        public static VerticalBox Get()
        {
            var box = ObjPool<VerticalBox>.Get();
            if (box.children == null)
                box.children = new List<Box>();
            return box;
        }

        public void Add(Box box)
        {
            children.Add(box);

            if (children.Count == 1)
            {
                height = box.height;
                depth = box.depth;
            }
            else
            {
                depth += box.height + box.depth;
            }
            RecalculateWidth(box);
        }

        public void Add(int position, Box box)
        {
            children.Insert(position, box);

            if (position == 0)
            {
                depth += box.depth + height;
                height = box.height;
            }
            else
            {
                depth += box.height + box.depth;
            }
            RecalculateWidth(box);
        }

        private void RecalculateWidth(Box box)
        {
            leftMostPos = Mathf.Min(leftMostPos, box.shift);
            rightMostPos = Mathf.Max(rightMostPos, box.shift + (box.width > 0 ? box.width : 0));
            width = rightMostPos - leftMostPos;
        }

        public void Shift(float upward)
        {
            height += upward;
            depth -= upward;
        }

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);

            var curY = y + height;
            var count = children.Count;
            if (ExtensionMode)
            {
                float offset = TEXConfiguration.main.ExtentPadding;
                for (int i = 0; i < count; i++)
                {
                    Box child = children[i];
                    curY -= child.height;
                    if (i > 0)
                        child.height += offset;
                    if (i < count - 1)
                        child.depth += offset;
                    child.Draw(drawingContext, scale, x + child.shift - leftMostPos, curY);
                    if (i > 0)
                        child.height -= offset;
                    if (i < count - 1)
                        child.depth -= offset;
                    curY -= child.depth;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    Box child = children[i];
                    curY -= child.height;
                    child.Draw(drawingContext, scale, x + child.shift - leftMostPos, curY);
                    curY -= child.depth;
                }
            }
        }

        public override void Flush()
        {
            for (int i = children.Count; i-- > 0;)
            {
                children[i].Flush();
            }
            children.Clear();

            Set(0, 0, 0, 0);
            leftMostPos = float.MaxValue;
            rightMostPos = float.MinValue;
            ExtensionMode = false;
            ObjPool<VerticalBox>.Release(this);
        }
    }
}
