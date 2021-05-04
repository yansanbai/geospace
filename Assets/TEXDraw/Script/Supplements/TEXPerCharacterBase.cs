using System.Text;
using System.Text.RegularExpressions;

namespace TexDrawLib
{
    public abstract class TEXPerCharacterBase : TEXDrawSupplementBase
    {
        public ScaleOffset m_Factor = ScaleOffset.identity;

        private static Regex m_pattern = new Regex(@"(\\\w+\s*\[.*?[^\\]\])|(\\\w+)|([^\s\\{}^_])|(\\[\\{}^_])");
        /*
			REGEX Pattern explanation ...
			there's 4 kind of group, if one of them match, then it'll captured into list:
			1. (\\\w+\s*\[.*?[^\\]\])   : Match kind like \cmd[] (with bracket) into one group (right now nested bracket isn't supported)
			2. (\\[\w]+)                : Match kind like \cmd (no bracket) into one group
			3. ([^\s\\{}^_])            : Match any character except spaces, or other preserved chars, separately
			4. (\\[\\{}^_])	            : Special case like \_, \^, etc. should be merged into one
		*/

        public override string ReplaceString(string original)
        {
            var reg = m_pattern.Matches(original);
            if (reg.Count == 0)
                return original;
            var sub = new StringBuilder(original);
            var count = (float)(reg.Count - 1);
            var offset = 0;
            var penalty = 0;
            OnBeforeSubtitution(count);
            for (int i = 0; i <= count; i++)
            {
                var mac = reg[i];
                if (mac.Length > 2)
                {
                    // this is likely a \cmd so it's not eligible.
                    // add penalty so factor can be more accurate.
                    penalty++;
                    continue;
                }
                sub.Remove(mac.Index + offset, mac.Length);
                var subtituted = Subtitute(mac, m_Factor.Evaluate((i - penalty) / (count - penalty)), offset);
                sub.Insert(mac.Index + offset, subtituted);
                offset += subtituted.Length - mac.Length;
            }
            return sub.ToString();
        }

        protected virtual string Subtitute(string match, float factor)
        {
            return match;
        }

        protected virtual string Subtitute(Match match, float factor, int offset)
        {
            return Subtitute(match.Value, factor);
        }

        protected virtual void OnBeforeSubtitution(float count)
        {
        }
    }
}
