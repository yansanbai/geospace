using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UIConstants
{
    public static float ratio = 2;
    public static float PlaneSpaceing = 20 * ratio;
    public static float NavPlaneWidth = 100 * ratio;
    public static float ToolPlaneHeight = 80 * ratio;
    public static float StatePlaneWidth = 200 * ratio;
    public static float InputPlaneHeight = 50 * ratio;


    public static float TextFontSize = 20 * ratio;
    public static float TextFontSubRatio = 0.6f;
    public static int TextFontSubSize = Mathf.FloorToInt(TextFontSize * TextFontSubRatio);

    public static float InputCaretSize = 2 * ratio;
    public static float InputNumWidth = 50 * ratio;
    public static float InputSpacing = 10 * ratio;

    public static float ToolOpenButtonWidth = 40 * ratio;
    public static float ToolCloseButtonWidth = 15 * ratio;
    public static float ToolButtonWidth = 40 * ratio;
    public static float ToolButtonSpacing = 20 * ratio;
    public static float ToolTipSpacing = 10 * ratio;

    public static float ElementBottonWidth = 60 * ratio;
    public static float ElementBottonHeight = 60 * ratio;
    public static float ElementBoardSpacing = 20 * ratio;

    public static float StateCellHeight = 40 * ratio;

    public static string PointFormat = "0.###";
    public static string LengthFormat = "0.###";
    public static string AreaFormat = "0.###";
    public static string AngleFormat = "0.###°";

    public static string TextFont = "FZYouHJVFWTWH1";

    public static string SignFormat(string sign, int subSize)
    {
        string text = "";
        if (sign.Length >= 1)
            text = sign.Substring(0, 1);
        if (sign.Length == 2)
            text += "<size=" + subSize + ">" + sign[1] + "</size>";

        return text;

    }

}
