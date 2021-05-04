using UnityEngine;

namespace TexDrawLib
{
    // Clockwise Rotated Character, right now it's used mainly just for braces
    public class RotatedCharBox : Box
    {
        public static RotatedCharBox Get(CharBox ch)
        {
            // this old charbox will get flushed
            ch.Flush();
            return Get(ch.ch);
        }

        public static RotatedCharBox Get(TexChar ch)
        {
            var box = ObjPool<RotatedCharBox>.Get();
            var font = ch.font;
            box.i = font.index;

            switch (box.type = font.type)
            {
                case TexAssetType.Font:
                    var scl = TexContext.Scale;
                    var c = box.c = ((TexFont)font).GenerateFont(ch.characterIndex,
                        (int)(TexContext.Resolution * scl) + 1, TexContext.Style.value);
                    float ratio = scl / c.size;
                    box.Set(c.maxX * ratio, (-c.minX) * ratio, 0, (c.maxY - c.minY) * ratio);
                    return box;
                case TexAssetType.Sprite:
                    {
                        var b = (box.o = (TexSprite)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.z, s.x, s.w, s.y);
                    }
                    return box;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    {
                        var b = ((TexFontSigned)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.z, s.x, s.w, s.y);
                    }
                    return box;
#endif
                default:
                    return null;
            }
        }

        public int i;

        public CharacterInfo c;

        public TexSprite o;

        public float bearing;

        public float italic;

        private Rect uv;

        public TexAssetType type;

        private new void Set(float depth, float height, float bearing, float italic)
        {
            this.depth = depth;
            this.height = height;
            this.bearing = bearing;
            this.italic = italic;
            this.width = italic + bearing;
            this.shift = 0;
        }

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);

            // Draw character at given position.
            Vector2 vPos = new Vector2((x - bearing) * scale, (y - depth) * scale);
            Vector2 vSize = new Vector2((bearing + italic) * scale, totalHeight * scale);

            switch (type)
            {
                case TexAssetType.Font:
                    drawingContext.Draw(i, vPos, vSize,
                        c.uvBottomRight, c.uvTopRight, c.uvTopLeft, c.uvBottomLeft);

                    break;
                case TexAssetType.Sprite:
                    var uv = this.uv;
                    if (o.alphaOnly)
                    {
                        drawingContext.Draw(i, vPos, vSize,
                           uv.min, new Vector2(uv.xMax, uv.y),
                           uv.max, new Vector2(uv.x, uv.yMax));
                    }
                    else
                    {
                        //Using RGB? then the color should be black
                        //see the shader why it's happen to be like that
                        TexContext.Color.Push(new Color32(0, 0, 0, 1));
                        drawingContext.Draw(i, vPos, vSize,
                           new Vector2(uv.xMax, uv.y), uv.max,
                           new Vector2(uv.x, uv.yMax), uv.min);
                        TexContext.Color.Pop();
                    }
                    break;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    {
                        uv = this.uv;
                        drawingContext.Draw(i, vPos, vSize,
                           new Vector2(uv.xMax, uv.y), uv.max,
                           new Vector2(uv.x, uv.yMax), uv.min);
                    }
                    break;
#endif
            }
        }

        public override void Flush()
        {
            ObjPool<RotatedCharBox>.Release(this);
        }
    }
}
