using UnityEngine;

namespace TexDrawLib
{
    public class StrikeBox : Box
    {
        private const string NegateSkin = "negateskin";

        public enum StrikeMode
        {
            diagonal = 0,
            diagonalInverse = 1,
            horizontal = 2,
            doubleHorizontal = 3,
            underline = 4,
            overline = 5,
            vertical = 6,
            verticalInverse = 7
        }

        public float margin;
        public float thickness;
        public float offsetPlus;
        public float offsetMinus;
        public StrikeMode mode;

        public static StrikeBox Get(float Height, float Width, float Depth, float Margin, float Thickness, StrikeMode Mode, float OffsetM, float OffsetP)
        {
            var box = Get(Height, Width, Depth);
            box.margin = Margin;
            box.thickness = Thickness;
            box.mode = Mode;
            box.offsetPlus = OffsetP;
            box.offsetMinus = OffsetM;
            return box;
        }

        public static StrikeBox Get(float Height, float Width, float Depth)
        {
            var box = ObjPool<StrikeBox>.Get();
            box.Set(Width, Height, Depth, 0);
            return box;
        }

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);
            PrepareDraw(drawingContext);

            //This is for people who are nerdy about math
            float op = Mathf.Max(0, offsetPlus), om = Mathf.Min(0, offsetMinus);
            float opm = op - om;
            float angle;
            switch (mode)
            {
                case StrikeMode.diagonal: angle = Mathf.Atan2(totalHeight + margin * 2 - opm, width); break;
                case StrikeMode.diagonalInverse: angle = Mathf.Atan2(totalHeight + margin * 2 - opm, width); break;
                case StrikeMode.vertical: angle = Mathf.Atan2(totalHeight + margin * 2, width - opm); break;
                case StrikeMode.verticalInverse: angle = Mathf.Atan2(totalHeight + margin * 2, width - opm); break;
                default: angle = 0; break;
            }
            float s = Mathf.Sin(angle) * thickness, c = Mathf.Cos(angle) * thickness;
            float w = width, ww = width + margin, h = totalHeight / 2f + margin;
            Vector2[] v = new Vector2[4];
            y += totalHeight / 2f - depth;
            if ((int)mode > 1 && (int)mode < 6)
                y += op + om;
            switch (mode)
            {
                case StrikeMode.diagonal:
                    v[0] = new Vector2((x + w + s) * scale, (y + h - c + om) * scale); //Top-Left
                    v[1] = new Vector2((x + w - s) * scale, (y + h + c + om) * scale); //Top-Right
                    v[2] = new Vector2((x - s) * scale, (y - h + c + op) * scale); //Bottom-Right
                    v[3] = new Vector2((x + s) * scale, (y - h - c + op) * scale); //Bottom-Left
                    break;
                case StrikeMode.diagonalInverse:
                    v[0] = new Vector2((x + s) * scale, (y + h + c + om) * scale); //Top-Left
                    v[1] = new Vector2((x - s) * scale, (y + h - c + om) * scale); //Top-Right
                    v[2] = new Vector2((x + w - s) * scale, (y - h - c + op) * scale); //Bottom-Right
                    v[3] = new Vector2((x + w + s) * scale, (y - h + c + op) * scale); //Bottom-Left
                    break;
                case StrikeMode.horizontal:
                    y += TEXConfiguration.main.MiddleLineOffset;
                    v[0] = new Vector2((x - margin) * scale, (y + thickness) * scale); //Top-Left
                    v[1] = new Vector2((x + ww) * scale, (y + thickness) * scale); //Top-Right
                    v[2] = new Vector2((x + ww) * scale, (y - thickness) * scale); //Bottom-Right
                    v[3] = new Vector2((x - margin) * scale, (y - thickness) * scale); //Bottom-Left
                    break;
                case StrikeMode.doubleHorizontal:
                    float doubleOffset = TEXConfiguration.main.DoubleNegateMargin;
                    y += TEXConfiguration.main.MiddleLineOffset;
                    v[0] = new Vector2((x - margin) * scale, (y + thickness + doubleOffset) * scale); //Top-Left
                    v[1] = new Vector2((x + ww) * scale, (y + thickness + doubleOffset) * scale); //Top-Right
                    v[2] = new Vector2((x + ww) * scale, (y - thickness + doubleOffset) * scale); //Bottom-Right
                    v[3] = new Vector2((x - margin) * scale, (y - thickness + doubleOffset) * scale); //Bottom-Left
                    Draw(drawingContext, v);

                    v[0] = new Vector2((x - margin) * scale, (y + thickness - doubleOffset) * scale); //Top-Left
                    v[1] = new Vector2((x + ww) * scale, (y + thickness - doubleOffset) * scale); //Top-Right
                    v[2] = new Vector2((x + ww) * scale, (y - thickness - doubleOffset) * scale); //Bottom-Right
                    v[3] = new Vector2((x - margin) * scale, (y - thickness - doubleOffset) * scale); //Bottom-Left
                    break;
                case StrikeMode.underline:
                    y -= totalHeight / 2f - depth + thickness * 2 - TEXConfiguration.main.UnderLineOffset;
                    v[0] = new Vector2((x - margin) * scale, (y + thickness) * scale); //Top-Left
                    v[1] = new Vector2((x + ww) * scale, (y + thickness) * scale); //Top-Right
                    v[2] = new Vector2((x + ww) * scale, (y - thickness) * scale); //Bottom-Right
                    v[3] = new Vector2((x - margin) * scale, (y - thickness) * scale); //Bottom-Left
                    break;
                case StrikeMode.overline:
                    y += totalHeight / 2 + thickness * 2 + TEXConfiguration.main.OverLineOffset;
                    v[0] = new Vector2((x - margin) * scale, (y + thickness) * scale); //Top-Left
                    v[1] = new Vector2((x + ww) * scale, (y + thickness) * scale); //Top-Right
                    v[2] = new Vector2((x + ww) * scale, (y - thickness) * scale); //Bottom-Right
                    v[3] = new Vector2((x - margin) * scale, (y - thickness) * scale); //Bottom-Left
                    break;
                case StrikeMode.vertical:
                    v[0] = new Vector2((x + w + s + om) * scale, (y + h - c) * scale); //Top-Left
                    v[1] = new Vector2((x + w - s + om) * scale, (y + h + c) * scale); //Top-Right
                    v[2] = new Vector2((x - s + op) * scale, (y - h + c) * scale); //Bottom-Right
                    v[3] = new Vector2((x + s + op) * scale, (y - h - c) * scale); //Bottom-Left
                    break;
                case StrikeMode.verticalInverse:
                    v[0] = new Vector2((x + s + op) * scale, (y + h + c) * scale); //Top-Left
                    v[1] = new Vector2((x - s + op) * scale, (y + h - c) * scale); //Top-Right
                    v[2] = new Vector2((x + w - s + om) * scale, (y - h - c) * scale); //Bottom-Right
                    v[3] = new Vector2((x + w + s + om) * scale, (y - h + c) * scale); //Bottom-Left
                    break;
            }
            Draw(drawingContext, v);
        }

        private Vector2[] uv;
        private int fontIdx;

        private void PrepareDraw(DrawingContext context)
        {
            /*
            TexChar n = TEXPreference.main.GetChar(NegateSkin);
            CharacterInfo c = DrawingContext.GetCharInfo(n.fontIndex, (char)TEXPreference.TranslateChar(n.index), context.prefFontSize);
            fontIdx = n.fontIndex;
            uv = new Vector2[4]
            {
                c.uvBottomLeft,
                c.uvBottomRight,
                c.uvTopRight,
                c.uvTopLeft
            };
            */
            fontIdx = TexUtility.blockFontIndex;
            uv = new Vector2[4]
            {
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
            };
        }

        private void Draw(DrawingContext context, Vector2[] verts)
        {
            context.Draw(fontIdx, verts, uv);
        }

        public override void Flush()
        {
            ObjPool<StrikeBox>.Release(this);
        }
    }
}
