using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Trim Spaces")]
    [TEXSupHelpTip("Trim Empty spaces between lines")]
    public class TEXSupTrimSpaces : TEXDrawSupplementBase
    {
        public override string ReplaceString(string original)
        {
            //Not the best way to go (GC Hugger)
            var cout = s_split.SafeSplit(original, '\n');
            var buff = s_split.buffer;
            for (int i = 0; i < cout; i++)
            {
                buff[i] = buff[i].Trim();
            }
            return s_split.Join(cout, "\n");
        }

        private StringSplitter s_split;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (s_split == null)
                s_split = new StringSplitter(4);
        }
    }
}
