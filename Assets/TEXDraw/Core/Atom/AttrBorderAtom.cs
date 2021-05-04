using System.Linq;
using UnityEngine;

namespace TexDrawLib
{
    public class AttrBorderAtom : Atom
    {
        public static AttrBorderAtom Get(Atom baseAtom, string param, bool margin)
        {
            var atom = ObjPool<AttrBorderAtom>.Get();
            atom.baseAtom = baseAtom;
            atom.margin = margin;
            atom.Type = baseAtom.Type;
            Parse(param, out atom.thickness, out atom.radius, out atom.color);
            return atom;
        }

        private static void Parse(string param, out Vector4 thickness, out Vector4 radius, out Color color)
        {
            thickness = Vector4.one;
            radius = Vector4.zero;
            color = Color.clear;

            if (param == null) { thickness *= TexUtility.lineThickness; return; }

            int start = 0, end = 0;
            int thickstage = 0; // 0,1,2
            int radiusstage = 0; // 0,1,2
            float thickscale = TexUtility.lineThickness;
            float radiusscale = TexUtility.lineThickness;

            while (LocateArg(param, ref start, ref end))
            {
                var p = param.Substring(start, end - start);
                if (p.Contains('.'))
                {
                    // float
                    if (thickstage == 0)
                    {
                        float.TryParse(p, out thickscale);
                        thickstage++;
                    }
                    else if (radiusstage == 0)
                    {
                        float.TryParse(p, out radiusscale);
                        radiusstage++;
                    }
                }
                else if (ColorUtility.TryParseHtmlString(p, out color))
                {
                    // color
                }
                else
                {
                    // int serial
                    if (p.Length == 3)
                    {
                        //color
                        ColorUtility.TryParseHtmlString("#" + p, out color);
                    }
                    else if (thickstage < 2)
                    {
                        thickness = ParseVector(p);
                        thickstage = 2;
                    }
                    else if (radiusstage < 2)
                    {
                        radius = ParseVector(p);
                        radiusstage = 2;
                    }
                }
            }
            radius *= radiusscale;
            thickness *= thickscale;
        }

        private static Vector4 ParseVector(string digit)
        {
            Vector4 v = Vector4.one;
            if (digit.Length == 1) v = Vector4.one * (digit[0] - '0');
            else if (digit.Length == 2) v = new Vector4(digit[0] - '0', digit[1] - '0', digit[0] - '0', digit[1] - '0');
            else if (digit.Length == 4) v = new Vector4(digit[0] - '0', digit[1] - '0', digit[2] - '0', digit[3] - '0');
            return v;
        }

        private static bool LocateArg(string param, ref int start, ref int end)
        {
            // look word by word
            start = end;
            while (start < param.Length && param[start] == ' ')
                start++;
            end = start;
            while (end < param.Length && param[end] != ' ')
                end++;
            return start < param.Length;
        }

        public Vector4 thickness;
        public Vector4 radius;
        public Color color;
        public bool margin;
        public Atom baseAtom;

        public override Box CreateBox()
        {
            // radius will be implemented later

            var margin = this.margin ? TEXConfiguration.main.BackdropMargin * TexContext.Scale : 0;

            var box = baseAtom.CreateBox();

            var hbox = HorizontalBox.Get(box, box.width + margin * 2, TexAlignment.Center);

            if (thickness.x > 0)
            {
                var rule = HorizontalRule.Get(hbox.height, thickness.x, 0, hbox.depth);
                hbox.Add(0, rule);
                rule.Set(rule.width, rule.height + margin + thickness.w, rule.depth + margin + thickness.y, 0);
            }

            if (thickness.z > 0)
            {
                var rule = HorizontalRule.Get(hbox.height, thickness.z, 0, hbox.depth);
                hbox.Add(rule);
                rule.Set(rule.width, rule.height + margin + thickness.w, rule.depth + margin + thickness.y, 0);
            }

            var vbox = VerticalBox.Get(hbox, hbox.totalHeight + margin * 2, TexAlignment.Center);

            if (thickness.y > 0)
                vbox.Add(HorizontalRule.Get(thickness.y, vbox.width, 0));

            if (thickness.w > 0)
                vbox.Add(0, HorizontalRule.Get(thickness.w, vbox.width, 0));

            if (color != Color.clear)
            {
                vbox.Add(0, AttrColorBox.Get(0, color));
                vbox.Add(AttrColorBox.Get(3, color));
            }

            // readjust
            vbox.height = box.height + margin + thickness.w;
            vbox.depth = box.depth + margin + thickness.y;
            return vbox;
        }

        public override void Flush()
        {
            base.Flush();
            ObjPool<AttrBorderAtom>.Release(this);
        }
    }
}
