namespace TexDrawLib
{
    // Atom representing whitespace.
    public class SpaceAtom : Atom
    {
        public float width;
        public float height;
        public float depth;
        public StrutPolicy policy;

        public static SpaceAtom Get(bool isGlue, float Width, float Height, float Depth)
        {
            var atom = ObjPool<SpaceAtom>.Get();
            if (isGlue)
            {
                float factor = TexUtility.glueRatio;
                atom.width = Width * factor;
                atom.height = Height * factor;
                atom.depth = Depth * factor;
                atom.policy = StrutPolicy.Glue;
            }
            else
            {
                atom.width = Width;
                atom.height = Height;
                atom.depth = Depth;
                atom.policy = StrutPolicy.Misc;
            }
            return atom;
        }

        public static SpaceAtom Get(float Width, float Height, float Depth)
        {
            var atom = ObjPool<SpaceAtom>.Get();
            atom.width = Width;
            atom.height = Height;
            atom.depth = Depth;
            atom.policy = StrutPolicy.Misc;
            return atom;
        }

        public static SpaceAtom Get(bool space = true)
        {
            var atom = Get(space ? TexUtility.spaceWidth : 0, TexUtility.spaceHeight, TexUtility.spaceDepth);
            atom.policy = StrutPolicy.BlankSpace;
            return atom;
        }

        public override Box CreateBox()
        {
            float factor = TexContext.Scale;
            return StrutBox.Get((width + TexContext.Kerning.value) * factor,
                height * factor, depth * factor, 0, policy);
        }

        public static Box CreateGlueBox(CharType leftType, CharType rightType)
        {
            float width = TEXPreference.main.GetGlue(leftType, rightType) + TexContext.Kerning.value;
            if (width != 0)
                return TexUtility.GetBox(Get(true, width, 0, 0));
            return null;
        }

        public override void Flush()
        {
            ObjPool<SpaceAtom>.Release(this);
        }
    }
}
