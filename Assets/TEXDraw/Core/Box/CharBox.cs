using System;
using UnityEngine;

namespace TexDrawLib
{
    // Box representing single character.
    public class CharBox : Box
    {
        // Unicode character (where it didn't inside our library)
        public static CharBox Get(int font, char ch)
        {
            var C = TEXPreference.main.GetChar(font, ch);
            if (C == null)
            {
                var f = TEXPreference.main.fonts[font];

                if (f.type == TexAssetType.Font)
                {
                    // unicode
                    var box = ObjPool<CharBox>.Get();
                    box.i = font;
                    var scl = TexContext.Scale;
                    var c = box.c = ((TexFont)f).GenerateFont(ch,
                        (int)(TexContext.Resolution * scl) + 1, TexContext.Style.value);
                    float r = scl / c.size;
                    box.Set(-c.minY * r, c.maxY * r, -c.minX * r, c.maxX * r, c.advance * r);
                    return box;
                }
                else
                    // a sprite. simply no way to fix this!
                    throw new InvalidOperationException("Illegal Character! '" + ch + "' doesn't exist in " + TEXPreference.main.fonts[font].name);
            }
            else
                return Get(C);
        }

        public static CharBox Get(TexChar ch)
        {
            var box = ObjPool<CharBox>.Get();
            var font = ch.font;
            box.ch = ch;
            box.i = font.index;

            switch (box.type = font.type)
            {
                case TexAssetType.Font:
                    {
                        var scl = TexContext.Scale;
                        var c = box.c = ((TexFont)font).GenerateFont(ch.characterIndex,
                            (int)(TexContext.Resolution * scl) + 1, TexContext.Style.value);
                        float r = scl / c.size;
                        box.Set(-c.minY * r, c.maxY * r, -c.minX * r, c.maxX * r, c.advance * r);
                    }

                    return box;
                case TexAssetType.Sprite:
                    {
                        var b = (box.o = (TexSprite)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.y, s.w, s.x, s.z, s.x + s.z);
                    }
                    return box;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    {
                        var b = ((TexFontSigned)font).GenerateMetric(ch.characterIndex);
                        box.uv = b.uv; var s = b.size;
                        box.Set(s.y, s.w, s.x, s.z, s.x + s.z);
                    }
                    return box;
#endif
                default:
                    return null;
            }
        }

        public TexChar ch;

        private int i;

        private CharacterInfo c;

        private TexSprite o;

        private Rect uv;

        public float bearing;

        public float italic;

        public TexAssetType type;

        private void Set(float depth, float height, float bearing, float italic, float width)
        {
            this.depth = depth;
            this.height = height;
            this.bearing = bearing;
            this.italic = italic;
            this.width = width;
            this.shift = 0;
        }

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);

            // Draw character at given position.
            Vector3 vPos = new Vector3((x - bearing) * scale, (y - depth) * scale);
            Vector2 vSize = new Vector2((bearing + italic) * scale, totalHeight * scale);

            switch (type)
            {
                case TexAssetType.Font:
                    drawingContext.Draw(i, vPos, vSize,
                         c.uvBottomLeft, c.uvBottomRight, c.uvTopRight, c.uvTopLeft);
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
                        TexContext.Color.Push(new Color32(0, 0, 0, TexContext.Color.value.a));
                        drawingContext.Draw(i, vPos, vSize,
                           uv.min, new Vector2(uv.xMax, uv.y),
                           uv.max, new Vector2(uv.x, uv.yMax));
                        TexContext.Color.Pop();
                    }
                    break;
#if TEXDRAW_TMP
                case TexAssetType.FontSigned:
                    uv = this.uv;
                    drawingContext.Draw(i, vPos, vSize,
                       uv.min, new Vector2(uv.xMax, uv.y),
                       uv.max, new Vector2(uv.x, uv.yMax));
                    break;
#endif
            }
        }

        public override void Flush()
        {
            ObjPool<CharBox>.Release(this);
        }
    }
}
