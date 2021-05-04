using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Find Replace")]
    [TEXSupHelpTip("A Quick Find-Replace tool to make custom string behaviours", true)]
    public class TEXSupFindReplace : TEXDrawSupplementBase
    {
        public bool useRegex = true;
        public FindReplace[] patterns = new FindReplace[0];

        public override string ReplaceString(string original)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null)
                    original = patterns[i].Execute(original, useRegex);
            }
            return original;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

#if UNITY_EDITOR

        [ContextMenu("Export (To Clipboard)")]
        private void Export()
        {
            var s = new StringBuilder();
            for (int i = 0; i < patterns.Length; i++)
            {
                s.AppendLine(patterns[i].find);
                s.AppendLine(patterns[i].replace);
                s.AppendLine();
            }
            GUIUtility.systemCopyBuffer = s.ToString();
        }

        [ContextMenu("Import (From Clipboard)")]
        private void Import()
        {
            var s = GUIUtility.systemCopyBuffer;
            var i = 0;
            var l = new List<FindReplace>();
            var p = s.Replace("\r", "").Split('\n');
            if (string.IsNullOrEmpty(s))
                return;

            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Importing TEXSup");

            while (i < p.Length)
            {
                var t = p[i++];
                if (t.Length == 0)
                    continue;

                var f = new FindReplace();
                l.Add(f);
                f.find = (t);
                if (i < p.Length)
                    f.replace = p[i++];
            }
            patterns = l.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

#endif
    }
}
