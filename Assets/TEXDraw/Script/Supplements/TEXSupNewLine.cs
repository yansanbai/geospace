using System.Text.RegularExpressions;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup New Line")]
    [TEXSupHelpTip("Detect \\n for new line")]
    public class TEXSupNewLine : TEXDrawSupplementBase
    {
        private const string f = @"\\n(?=[^\d\w])(\s*)";
        private const string t = "\n";

        public override string ReplaceString(string original)
        {
            return Replace(original);
        }

        static public string Replace(string original)
        {
            //This will recognize \n as new line
            return Regex.Replace(original, f, t);
        }
    }
}
