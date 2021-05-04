using System.Collections.Generic;
using UnityEngine;

namespace TexDrawLib
{
    // Atom representing horizontal row of other atoms, separated by glue.
    public class RowAtom : Atom
    {
        public static RowAtom Get(IList<TexFormula> formulaList)
        {
            var atom = Get();
            foreach (var formula in formulaList)
            {
                if (formula.RootAtom != null)
                    atom.Elements.Add(formula.RootAtom);
            }
            return atom;
        }

        public static RowAtom Get(Atom baseAtom)
        {
            var atom = Get();
            if (baseAtom != null)
            {
                if (baseAtom is RowAtom)
                {
                    var els = ((RowAtom)baseAtom).Elements;
                    for (int i = 0; i < els.Count; i++)
                        atom.Elements.Add(els[i]);
                }
                else
                    atom.Elements.Add(baseAtom);
            }
            return atom;
        }

        public static RowAtom Get()
        {
            return ObjPool<RowAtom>.Get();
        }

        public List<Atom> Elements = new List<Atom>();

        public void Add(Atom atom, bool preferUnwrap = false)
        {
            if (atom != null)
            {
                if (preferUnwrap && atom is RowAtom)
                {
                    var els = ((RowAtom)atom).Elements;
                    for (int i = 0; i < els.Count; i++)
                    {
                        Elements.Add(els[i]);
                    }
                    els.Clear();
                    atom.Flush();
                }
                else
                    Elements.Add(atom);
            }
        }

        public override Box CreateBox()
        {
            // Create result box.
            var resultBox = HorizontalBox.Get();
            Atom curAtom = null, prevAtom = null;
            var resultPos = 0;
            // Create and add box for each atom in row.
            for (int i = 0; i < Elements.Count; i++)
            {
                curAtom = (Elements[i]);

                // Create and add glue box, unless atom is first of row or previous/current atom is spaces.
                if (prevAtom != null && !(prevAtom is SpaceAtom) && !(curAtom is SpaceAtom))
                {
                    Box spaceBox = SpaceAtom.CreateGlueBox(prevAtom.RightType, curAtom.LeftType);
                    if (spaceBox != null)
                    {
                        resultBox.Add(spaceBox);
                        resultPos++;
                    }
                }
                // Create and add box for atom.
                GenerateDelimiterBox(resultBox, ref i, ref resultPos);
                //resultBox.Add(curBox);
                prevAtom = curAtom;
            }
            return resultBox;
        }

        private Box lastGeneratedBox;

        public Box GenerateDelimiterBox(HorizontalBox result, ref int elementPos, ref int resultPos)
        {
            var curAtom = Elements[elementPos];

            if (!(curAtom is CharSymbol) || !((CharSymbol)curAtom).IsDelimiter)
            {
                // This is not delimiter, hence just create and do nothing.
                var box = curAtom.CreateBox();
                if (curAtom.Type == CharType.BigOperator) // specific patch to BigOperator
                    TexUtility.CentreBox(box);
                if (box is HorizontalBox && (curAtom is AttrSizeAtom || curAtom is AttrStyleAtom)) // specific patch to Atoms that should be wrappable.
                {
                    var h = box as HorizontalBox;
                    result.AddRange(h, resultPos);
                    resultPos += h.children.Count;
                }
                else
                    result.Add(resultPos++, box);
                lastGeneratedBox = box;
                return box;
            }

            var nextAtom = elementPos + 1 < Elements.Count ? Elements[elementPos + 1] : null;
            var prevAtom = elementPos > 0 ? Elements[elementPos - 1] : null;

            var minHeight = 0f;
            var ourPos = resultPos;
            if (nextAtom != null && curAtom.RightType == CharType.OpenDelimiter)
            {
                elementPos++;
                var nextBox = GenerateDelimiterBox(result, ref elementPos, ref resultPos);
                minHeight = nextBox.totalHeight;
            }
            else if (lastGeneratedBox != null && curAtom.LeftType == CharType.CloseDelimiter)
            {
                var prevBox = lastGeneratedBox;
                minHeight = prevBox.totalHeight;
            }
            else
            {
                if (prevAtom != null && lastGeneratedBox != null)
                {
                    var prevBox = lastGeneratedBox;
                    minHeight = prevBox.totalHeight;
                }
                if (nextAtom != null)
                {
                    elementPos++;
                    var nextBox = GenerateDelimiterBox(result, ref elementPos, ref resultPos);
                    minHeight = Mathf.Max(nextBox.totalHeight, minHeight);
                }
            }
            var curBox = curAtom is SymbolAtom ? (((SymbolAtom)curAtom).CreateBox(minHeight)) : curAtom.CreateBox();
            TexUtility.CentreBox(curBox);
            result.Add(ourPos, curBox);
            if (ourPos == resultPos)
                lastGeneratedBox = curBox;
            resultPos++;
            return curBox;
        }

        public override CharType LeftType
        {
            get
            {
                if (Elements.Count == 0)
                    return Type;
                return Elements[0].LeftType;
            }
        }

        public override CharType RightType
        {
            get
            {
                if (Elements.Count == 0)
                    return Type;
                return Elements[Elements.Count - 1].RightType;
            }
        }

        public override void Flush()
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Flush();
            }
            Elements.Clear();
            ObjPool<RowAtom>.Release(this);
        }
    }
}
