namespace TexDrawLib
{
    // Box representing other box with horizontal rule above it.
    public class OverBar : VerticalBox
    {
        public static VerticalBox Get(Box box, float kern, float thickness)
        {
            var atom = Get();
            atom.Add(HorizontalRule.Get(thickness, box.width, box.shift));
            kern += thickness;
            atom.Add(StrutBox.Get(0, kern / 2, 0, 0));
            atom.Add(box);
            atom.Add(StrutBox.Get(0, kern / 2, 0, 0));
            return atom;
        }
    }
}
