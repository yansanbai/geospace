using System.Collections.Generic;

using UnityEngine;

namespace TexDrawLib
{
    public class MatrixAtom : Atom
    {
        public MatrixAtom()
            : base()
        {
            Type = CharTypeInternal.Inner;
            Elements = ListPool<List<Atom>>.Get();
        }

        public static MatrixAtom Get()
        {
            var atom = ObjPool<MatrixAtom>.Get();
            atom.horizontalAlign = 1 + 8 + 64;
            atom.verticalAlign = 1 + 8 + 64;
            atom.horizontalLine = 0;
            atom.verticalLine = 0;
            return atom;
        }

        public List<List<Atom>> Elements;

        public void Add(List<Atom> atom)
        {
            if (atom != null)
                Elements.Add(atom);
        }

        ///1 = Center, 2 = Left, 4 = Right (First Row)
        ///8 = Center, 16 = Left, 32 = Right (First Column)
        ///64 = Center, 128 = Left, 256 = Right (Body)
        public int horizontalAlign = 1 + 8 + 64;

        ///1 = Center, 2 = Left, 4 = Right (First Row)
        ///8 = Center, 16 = Left, 32 = Right (First Column)
        ///61 = Center, 128 = Left, 256 = Right (Body)
        public int verticalAlign = 1 + 8 + 64;

        ///column-by-column bit masks
        public int horizontalLine;

        ///1 = Outside, 2 = First, 4 = Inside, 8 = First is bold, 16 = Outside is bold
        public int verticalLine;

        public override Box CreateBox()
        {
            List<List<Box>> boxes = ListPool<List<Box>>.Get();
            List<float> boxesHeight = ListPool<float>.Get();
            List<float> boxesShift = ListPool<float>.Get();
            List<float> boxesWidth = ListPool<float>.Get();

            TexContext.Environment.Push(TexUtility.GetCrampedStyle());

            float padding = TEXConfiguration.main.MatrixMargin * TexContext.Scale;

            for (int i = 0; i < Elements.Count; i++)
            {
                boxes.Add(ListPool<Box>.Get());
                float h = 0f, d = 0f;
                for (int j = 0; j < Elements[i].Count; j++)
                {
                    Box box;
                    if (Elements[i][j] != null)
                        box = (Elements[i][j].CreateBox());
                    else
                        box = (ObjPool<StrutBox>.Get());
                    boxes[i].Add(box);
                    if (j >= boxesWidth.Count)
                        boxesWidth.Add(box.width);
                    else
                        boxesWidth[j] = Mathf.Max(boxesWidth[j], box.width);
                    h = Mathf.Max(h, box.height);
                    d = Mathf.Max(d, box.depth);
                }
                boxesHeight.Add(Mathf.Max(h + d, padding / 2f));
                boxesShift.Add(h);
            }

            bool outsideGap = (horizontalLine > 0 && (enumContains(horizontalLine, 0) || enumContains(horizontalLine, 4)))
                || (verticalLine > 0 && (enumContains(verticalLine, 0) || enumContains(verticalLine, 4)));

            var vBox = VerticalBox.Get();
            Box resultBox = vBox;

            Box kern = null;
            Box kernHalf = null;
            if (boxesWidth.Count > 1 || boxesHeight.Count > 1)
                kern = StrutBox.Get(padding, padding, 0, 0);
            if (outsideGap)
            {
                kernHalf = StrutBox.Get(padding / 2f, padding / 2f, 0, 0);
                vBox.Add(kernHalf);
            }

            TexAlignment firstRowH = alterH(horizontalAlign % 8);
            TexAlignment firstRowV = alterV(verticalAlign % 8);
            TexAlignment firstColH = alterH((horizontalAlign >> 3) % 8);
            TexAlignment firstColV = alterV((verticalAlign >> 3) % 8);
            TexAlignment bodyH = alterH((horizontalAlign >> 6) % 8);
            TexAlignment bodyV = alterV((verticalAlign >> 6) % 8);
            for (int i = 0; i < Elements.Count; i++)
            {
                var list = HorizontalBox.Get();
                if (outsideGap)
                    list.Add(kernHalf);
                for (int j = 0; j < Elements[i].Count; j++)
                {
                    if (i == 0)
                        list.Add(VerticalBox.Get(HorizontalBox.Get(boxes[i][j], boxesWidth[j], firstRowH), boxesHeight[i], firstRowV));
                    else if (j == 0)
                        list.Add(VerticalBox.Get(HorizontalBox.Get(boxes[i][j], boxesWidth[j], firstColH), boxesHeight[i], firstColV));
                    else
                        list.Add(VerticalBox.Get(HorizontalBox.Get(boxes[i][j], boxesWidth[j], bodyH), boxesHeight[i], bodyV));

                    if (j < Elements[i].Count - 1)
                        list.Add(kern);
                    else if (outsideGap)
                        list.Add(kernHalf);
                }
                list.depth = boxesHeight[i] - list.height;
                vBox.Add(list);
                if (i < Elements.Count - 1)
                    vBox.Add(kern);
                else if (outsideGap)
                    vBox.Add(kernHalf);
            }

            var lineThick = TEXConfiguration.main.LineThickness * TexContext.Scale;

            //Add horizontal lines for table
            if (horizontalLine > 0)
            {
                var outside = enumContains(horizontalLine, 0);
                var first = enumContains(horizontalLine, 1);
                var inset = enumContains(horizontalLine, 2);
                var firstThick = enumContains(horizontalLine, 3);
                var outsideThick = enumContains(horizontalLine, 4);

                float gapX = (padding - lineThick);
                float gapXThick = (padding - (lineThick * 2));
                float gapXNone = (padding);

                float gapOutside = (outside ? (outsideThick ? gapXThick : gapX) : gapXNone);
                float gapInset = (inset ? gapX : gapXNone);
                float lineOutside = outsideThick ? lineThick * 2 : lineThick;
                var insideBox = resultBox;

                var hBox = HorizontalBox.Get(resultBox);
                resultBox = hBox;

                if (outsideGap)
                    hBox.Add(StrutBox.Get(-insideBox.width - lineOutside / 2f, 0, 0, 0));
                else
                    hBox.Add(StrutBox.Get(-insideBox.width - lineOutside * 1.5f, 0, 0, 0));

                for (int i = 0; i < boxesWidth.Count; i++)
                {
                    if (i == 0)
                    {
                        if (outside)
                            hBox.Add(HorizontalRule.Get(insideBox.height, lineOutside, 0, insideBox.depth));
                        hBox.Add(StrutBox.Get(boxesWidth[i] + gapOutside, 0, 0, 0));
                        continue;
                    }
                    if (i == 1)
                    {
                        if (first)
                            hBox.Add(HorizontalRule.Get(insideBox.height, firstThick ? lineThick * 2 : lineThick, 0, insideBox.depth));
                        hBox.Add(StrutBox.Get(boxesWidth[i] + (first ? (firstThick ? gapXThick : gapX) : gapXNone), 0, 0, 0));
                        continue;
                    }
                    if (inset)
                        hBox.Add(HorizontalRule.Get(insideBox.height, lineThick, 0, insideBox.depth));
                    hBox.Add(StrutBox.Get(boxesWidth[i] + gapInset, 0, 0, 0));
                }
                if (outside)
                    hBox.Add(HorizontalRule.Get(insideBox.height, lineOutside, 0, insideBox.depth));
            }

            if (verticalLine > 0)
            {
                var outside = enumContains(verticalLine, 0);
                var first = enumContains(verticalLine, 1);
                var inset = enumContains(verticalLine, 2);
                var firstThick = enumContains(verticalLine, 3);
                var outsideThick = enumContains(verticalLine, 4);

                float gapX = (padding - lineThick);
                float gapXThick = (padding - (lineThick * 2));
                float gapXNone = (padding);

                float gapOutside = (outside ? (outsideThick ? gapXThick : gapX) : gapXNone);
                float gapInset = (inset ? gapX : gapXNone);
                float lineOutside = outsideThick ? lineThick * 2 : lineThick;
                var insideBox = resultBox;
                var size = insideBox.width;

                vBox = VerticalBox.Get(resultBox);
                resultBox = vBox;

                if (outsideGap)
                    vBox.Add(StrutBox.Get(0, -insideBox.totalHeight - lineOutside / 2f, 0, 0));
                else
                    vBox.Add(StrutBox.Get(0, -insideBox.totalHeight, 0, 0));

                for (int i = 0; i < boxesHeight.Count; i++)
                {
                    if (i == 0)
                    {
                        if (outside)
                            vBox.Add(HorizontalRule.Get(lineOutside, size, 0));
                        vBox.Add(StrutBox.Get(0, boxesHeight[i] + (outsideGap ? gapOutside : gapOutside / 2f), 0, 0));
                    }
                    else if (i == 1)
                    {
                        if (first)
                            vBox.Add(HorizontalRule.Get(firstThick ? lineThick * 2 : lineThick, size, 0));
                        var thick = (first ? (firstThick ? gapXThick : gapX) : gapXNone);
                        vBox.Add(StrutBox.Get(0, boxesHeight[i] + (boxesHeight.Count == 2 && !outsideGap ? thick / 2f : thick), 0, 0));
                    }
                    else
                    {
                        if (inset)
                            vBox.Add(HorizontalRule.Get(lineThick, size, 0));
                        vBox.Add(StrutBox.Get(0, boxesHeight[i] + (i < boxesHeight.Count - 1 || outsideGap ? gapInset : gapInset / 2f), 0, 0));
                    }
                }
                if (outside && outsideGap)
                    vBox.Add(HorizontalRule.Get(lineOutside, size, 0, 0));
            }

            TexUtility.CentreBox(resultBox);

            TexContext.Environment.Pop();

            //Clear resources
            ListPool<float>.Release(boxesHeight);
            ListPool<float>.Release(boxesWidth);
            ListPool<float>.Release(boxesShift);
            for (int i = 0; i < boxes.Count; i++)
            {
                ListPool<Box>.ReleaseNoFlush(boxes[i]);
            }
            ListPool<List<Box>>.ReleaseNoFlush(boxes);

            return resultBox;
        }

        private static bool enumContains(int v, int toMatchPow)
        {
            int pow = 1 << toMatchPow;
            return (v & pow) == pow;
        }

        public override void Flush()
        {
            if (Elements != null)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    ListPool<Atom>.Release(Elements[i]);
                }
                Elements.Clear();
            }
            ObjPool<MatrixAtom>.Release(this);
        }

        private static TexAlignment alterH(int v)
        {
            if (v == 2)
                return TexAlignment.Left;
            if (v == 4)
                return TexAlignment.Right;
            return TexAlignment.Center;
        }

        private static TexAlignment alterV(int v)
        {
            if (v == 2)
                return TexAlignment.Top;
            if (v == 4)
                return TexAlignment.Bottom;
            return TexAlignment.Center;
        }

        public static void ParseMatrix(List<Atom> el, List<List<Atom>> childs)
        {
            Last(childs).Add(RowAtom.Get());
            foreach (Atom a in el)
            {
                if (a is SymbolAtom)
                {
                    var b = ((SymbolAtom)a);
                    if (b.Name == "ampersand")
                    {
                        Last(childs).Add(RowAtom.Get());
                        b.Flush();
                    }
                    else if (b.Name == "mid")
                    {
                        childs.Add(ListPool<Atom>.Get());
                        Last(childs).Add(RowAtom.Get());
                        b.Flush();
                    }
                    else
                        ((RowAtom)Last(Last(childs))).Add(a);
                }
                else if (a is CharAtom)
                {
                    var b = ((CharAtom)a);
                    if (b.Character == 0x26)
                    {
                        Last(childs).Add(RowAtom.Get());
                        b.Flush();
                    }
                    else if ((b.Character == 0x7c))
                    {
                        childs.Add(ListPool<Atom>.Get());
                        Last(childs).Add(RowAtom.Get());
                        b.Flush();
                    }
                    else
                        ((RowAtom)Last(Last(childs))).Add(a);
                }
                else
                    ((RowAtom)Last(Last(childs))).Add(a);
            }
        }

        public static T Last<T>(List<T> list) where T : class
        {
            if (list.Count > 0)
                return list[list.Count - 1];
            return null;
        }

        public static void ParseMatrixVertical(List<Atom> el, List<List<Atom>> childs)
        {
            Last(childs).Add(ObjPool<RowAtom>.Get());
            int vPool = 0, hPool = 0;
            foreach (Atom a in el)
            {
                if (a is SymbolAtom)
                {
                    var b = ((SymbolAtom)a);
                    if (b.Name == "ampersand")
                    {
                        hPool++;
                        Last(childs).Add(RowAtom.Get());
                        vPool = 0;
                        for (int i = 0; i < childs.Count; i++)
                        {
                            if (childs[i].Count < hPool + 1)
                                childs[i].Add(RowAtom.Get());
                        }
                        b.Flush();
                    }
                    else if (b.Name == "mid")
                    {
                        vPool++;
                        if (childs.Count <= vPool)
                        {
                            childs.Add(new List<Atom>());
                            while (Last(childs).Count < hPool + 1)
                                Last(childs).Add(RowAtom.Get());
                        }
                        b.Flush();
                    }
                }
                else if (a is CharAtom)
                {
                    var b = ((CharAtom)a);
                    if (b.Character == 0x26/*ampersand*/)
                    {
                        hPool++;
                        Last(childs).Add(RowAtom.Get());
                        vPool = 0;
                        for (int i = 0; i < childs.Count; i++)
                        {
                            if (childs[i].Count < hPool + 1)
                                childs[i].Add(RowAtom.Get());
                        }
                        b.Flush();
                    }
                    else if (b.Character == 0x7c)
                    {
                        vPool++;
                        if (childs.Count <= vPool)
                        {
                            childs.Add(new List<Atom>());
                            while (Last(childs).Count < hPool + 1)
                                Last(childs).Add(RowAtom.Get());
                        }
                        b.Flush();
                    }
                    else
                        ((RowAtom)Last(childs[vPool])).Add(a);
                }
                else
                    ((RowAtom)Last(childs[vPool])).Add(a);
            }
        }
    }
}
