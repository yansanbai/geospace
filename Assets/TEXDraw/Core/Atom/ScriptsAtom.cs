using UnityEngine;

// Atom representing scripts to attach to other atom.
namespace TexDrawLib
{
    public class ScriptsAtom : Atom
    {
        private static SpaceAtom scriptSpaceAtom
        {
            get
            {
                return SpaceAtom.Get(TexUtility.glueRatio, 0, 0);
            }
        }

        public static ScriptsAtom Get(Atom baseAtom, Atom subscriptAtom, Atom superscriptAtom)
        {
            var atom = ObjPool<ScriptsAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.SubscriptAtom = subscriptAtom;
            atom.SuperscriptAtom = superscriptAtom;
            atom.Type = CharTypeInternal.Inner;
            return atom;
        }

        public Atom BaseAtom;

        public Atom SubscriptAtom;

        public Atom SuperscriptAtom;

        public override Box CreateBox()
        {
            // Create box for base atom.

            // Save it shift and use it later

            // Create result box.

            // Set delta value and preliminary shift-up and shift-down amounts depending on type of base atom.
            float shiftUp = 0, shiftDown = 0;

            Box baseBox; HorizontalBox resultBox;

            if (SubscriptAtom == null && SuperscriptAtom == null)
                return (BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox());
            if (BaseAtom is AccentedAtom)
            {
                TexContext.Environment.Push(TexUtility.GetCrampedStyle());
                baseBox = ((AccentedAtom)BaseAtom).BaseAtom.CreateBox();
                TexContext.Environment.Pop();
            }
            else
            {
                baseBox = (BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox());
                //if (BaseAtom is CharSymbol)
                //{
                //    var delta = 0f;
                //    var ch = ((CharSymbol)BaseAtom).GetChar();
                //    if (!((CharSymbol)BaseAtom).IsTextSymbol)
                //        delta =  ch.italix + bearing - width;
                //    if (delta > TexUtility.FloatPrecision && SubscriptAtom == null)
                //    {
                //        resultBox.Add(StrutBox.Get(delta, 0, 0, 0));
                //        delta = 0;
                //    }
                //}
            }

            resultBox = HorizontalBox.Get(baseBox);

            var shift = baseBox.shift;

            Box superscriptBox = null;
            HorizontalBox superscriptContainerBox = null;
            Box subscriptBox = null;
            HorizontalBox subscriptContainerBox = null;

            TexContext.Environment.Push(TexUtility.GetSuperscriptStyle());
            shiftUp = baseBox.height - TEXConfiguration.main.SupDrop * TexContext.Scale;

            if (SuperscriptAtom != null)
            {
                // Create box for superscript atom.
                superscriptBox = SuperscriptAtom.CreateBox();
                superscriptContainerBox = HorizontalBox.Get(superscriptBox);

                // Add box for script space.
                superscriptContainerBox.Add(scriptSpaceAtom.CreateBox());

                // Adjust shift-up amount.
                float p;
                if (TexContext.Environment.value == TexEnvironment.Display)
                    p = TEXConfiguration.main.SupMin * TexContext.Scale;
                else
                    p = TEXConfiguration.main.SupMinNarrow * TexContext.Scale;

                shiftUp = Mathf.Max(shiftUp, p);
            }

            TexContext.Environment.Pop();

            TexContext.Environment.Push(TexUtility.GetSubscriptStyle());
            shiftDown = baseBox.depth + TEXConfiguration.main.SubDrop * TexContext.Scale;

            if (SubscriptAtom != null)
            {
                // Create box for subscript atom.
                subscriptBox = SubscriptAtom.CreateBox();
                subscriptContainerBox = HorizontalBox.Get(subscriptBox);

                // Add box for script space.
                subscriptContainerBox.Add(scriptSpaceAtom.CreateBox());
            }

            TexContext.Environment.Pop();

            // Check if only superscript is set.
            if (subscriptBox == null)
            {
                superscriptContainerBox.shift = -shiftUp;
                resultBox.Add(superscriptContainerBox);
                resultBox.height = shiftUp + superscriptBox.height;
                return resultBox;
            }

            // Check if only subscript is set.
            if (superscriptBox == null)
            {
                subscriptBox.shift = Mathf.Max(shiftDown, TEXConfiguration.main.SubMinNoSup * TexContext.Scale);
                resultBox.Add(subscriptContainerBox);
                resultBox.depth = shiftDown + subscriptBox.depth;
                return resultBox;
            }

            // Adjust shift-down amount.
            shiftDown = Mathf.Max(shiftDown, TEXConfiguration.main.SubMinOnSup * TexContext.Scale);

            // Space between subscript and superscript.
            float scriptsInterSpace = shiftUp - superscriptBox.depth + shiftDown - subscriptBox.height;

            scriptsInterSpace = shiftUp - superscriptBox.depth + shiftDown - subscriptBox.height;

            // If baseAtom is null, make it right-aligned

            if (BaseAtom is SpaceAtom && ((SpaceAtom)BaseAtom).policy == StrutPolicy.Misc)
            {
                var max = Mathf.Max(superscriptContainerBox.width, subscriptContainerBox.width);
                if (superscriptContainerBox.width < max)
                    superscriptContainerBox.Add(0, StrutBox.Get(max - superscriptContainerBox.width, 0, 0, 0));
                if (subscriptContainerBox.width < max)
                    subscriptContainerBox.Add(0, StrutBox.Get(max - subscriptContainerBox.width, 0, 0, 0));
            }

            // Create box containing both superscript and subscript.
            var scriptsBox = VerticalBox.Get();
            scriptsBox.Add(superscriptContainerBox);
            scriptsBox.Add(StrutBox.Get(0, scriptsInterSpace, 0, 0));
            scriptsBox.Add(subscriptContainerBox);
            scriptsBox.height = shiftUp + superscriptBox.height;
            scriptsBox.depth = shiftDown + subscriptBox.depth;
            scriptsBox.shift = shift;
            resultBox.Add(scriptsBox);

            return resultBox;
        }

        public override CharType LeftType { get { return base.LeftType; } }

        public override CharType RightType { get { return base.RightType; } }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }

            if (SuperscriptAtom != null)
            {
                SuperscriptAtom.Flush();
                SuperscriptAtom = null;
            }

            if (SubscriptAtom != null)
            {
                SubscriptAtom.Flush();
                SubscriptAtom = null;
            }
            ObjPool<ScriptsAtom>.Release(this);
        }
    }
}
