using System;

namespace TexDrawLib
{
    // Creates boxes containing delimeter symbol that exists in different sizes.
    public static class DelimiterFactory
    {
        public static Box CreateBox(string symbol, float minHeight)
        {
            minHeight += TEXConfiguration.main.DelimiterRecursiveOffset;
            var ch = TEXPreference.main.GetChar(symbol).GetMetric();

            // Find first version of character that has at least minimum height.
            var totalHeight = ch.height + ch.depth;

            while (totalHeight <= minHeight && ch.ch.nextLargerExist)
            {
                ch.Flush();
                ch = ch.ch.nextLarger.GetMetric();
                totalHeight = ch.height + ch.depth;
            }

            if (totalHeight < minHeight && ch.ch.extensionExist && !ch.ch.extensionHorizontal)
            {
                var resultBox = VerticalBox.Get();
                resultBox.ExtensionMode = true;
                // Construct box from extension character.
                ch.Flush();
                var ext = ch.ch.GetExtentMetrics();
                if (ext[0] != null) resultBox.Add(ext[0]);
                if (ext[1] != null) resultBox.Add(ext[1]);
                if (ext[2] != null) resultBox.Add(ext[2]);

                // Insert repeatable part multiple times until box is high enough.
                if (ext[3] != null)
                {
                    var repeatBox = ext[3];
                    if (repeatBox.totalHeight <= 0)
                        throw new ArgumentOutOfRangeException("PULL CHAR DEL ZERO");
                    while (resultBox.height + resultBox.depth <= minHeight)
                    {
                        if (ext[0] != null && ext[2] != null)
                        {
                            resultBox.Add(1, repeatBox);
                            if (ext[1] != null)
                                resultBox.Add(resultBox.children.Count - 1, repeatBox);
                        }
                        else if (ext[2] != null)
                            resultBox.Add(0, repeatBox);
                        else
                            resultBox.Add(repeatBox);
                    }
                }
                return resultBox;
            }
            else
                // just enough
                return ch;
        }

        public static Box CreateBoxHorizontal(string symbol, float minWidth)
        {
            minWidth += TEXConfiguration.main.DelimiterRecursiveOffset;
            var ch = TEXPreference.main.GetChar(symbol).GetMetric();

            var ch2 = TEXPreference.main.GetChar(symbol);
            bool isAlreadyHorizontal = true;

            while (ch2 != null)
            {
                if (ch2.extensionExist && !ch2.extensionHorizontal)
                {
                    isAlreadyHorizontal = false;
                    break;
                }
                ch2 = ch2.nextLarger;
            }

            // Find first version of character that has at least minimum width.
            var totalWidth = isAlreadyHorizontal ? ch.bearing + ch.italic : ch.totalHeight;
            while (totalWidth < minWidth && ch.ch.nextLargerExist)
            {
                ch.Flush();
                ch = ch.ch.nextLarger.GetMetric();
                totalWidth = isAlreadyHorizontal ? ch.bearing + ch.italic : ch.totalHeight;
            }

            if (totalWidth < minWidth && ch.ch.extensionExist)
            {
                var resultBox = HorizontalBox.Get();
                resultBox.ExtensionMode = true;
                // Construct box from extension character.
                var ext = ch.ch.GetExtentMetrics();
                if (isAlreadyHorizontal)
                {
                    if (ext[0] != null) resultBox.Add(ext[0]);
                    if (ext[1] != null) resultBox.Add(ext[1]);
                    if (ext[2] != null) resultBox.Add(ext[2]);
                }
                else
                {
                    if (ext[2] != null) resultBox.Add(RotatedCharBox.Get(ext[2]));
                    if (ext[1] != null) resultBox.Add(RotatedCharBox.Get(ext[1]));
                    if (ext[0] != null) resultBox.Add(RotatedCharBox.Get(ext[0]));
                }

                // Insert repeatable part multiple times until box is high enough.
                if (ext[3] != null)
                {
                    Box repeatBox = isAlreadyHorizontal ? (Box)ext[3] : RotatedCharBox.Get(ext[3]);
                    do
                    {
                        if (ext[0] != null && ext[2] != null)
                        {
                            resultBox.Add(1, repeatBox);
                            if (ext[1] != null)
                                resultBox.Add(resultBox.children.Count - 1, repeatBox);
                        }
                        else if (ext[2] != null)
                            resultBox.Add(0, repeatBox);
                        else
                            resultBox.Add(repeatBox);
                    }
                    while (resultBox.width < minWidth);
                }
                return resultBox;
            }
            else
            {
                // Just enough
                if (isAlreadyHorizontal)
                    return ch;
                else
                    return RotatedCharBox.Get(ch.ch);
            }
        }
    }
}
