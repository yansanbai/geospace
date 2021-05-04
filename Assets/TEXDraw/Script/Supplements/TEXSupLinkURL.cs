using System.Text.RegularExpressions;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Auto Link URL")]
    [TEXSupHelpTip("Auto detect URL and email links. Requires TEXLink")]
    public class TEXSupLinkURL : TEXDrawSupplementBase
    {
        public string commands = @"\ulink";
        private const string f = @"(https?:\/\/)?(mailto:)?([@\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?";

        public override string ReplaceString(string original)
        {
            string dest = "{" + commands + " $&}";
            //This will give \link to any detected URL (and email)
            //WARNING: Slow
            return Regex.Replace(original, f, dest, RegexOptions.IgnoreCase);
        }
    }
}
