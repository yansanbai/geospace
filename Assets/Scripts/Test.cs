using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float a;
    public float b;
    public float c;
    public float d;
    public float e;
    public float f;

    public Range<float> TriPyramidEdgeRange(float a, float b, float c, float d, float e)
    {
        float h_bc = TriangleTall(b, c, a);
        float h_de = TriangleTall(d, e, a);

        Debug.Log("h_bc = " + h_bc + "  h_de = " + h_de);

        float l_bc = Math.AboutEquals(b * b, h_bc * h_bc) ? 0 : Mathf.Sqrt(b * b - h_bc * h_bc);
        float l_de = Math.AboutEquals(d * d, h_de * h_de) ? 0 : Mathf.Sqrt(d * d - h_de * h_de);

        Debug.Log("???: " + (d * d - h_de * h_de));
        float l = Mathf.Abs(l_bc - l_de);
        Debug.Log("l_bc = " + l_bc + "  l_de = " + l_de);
        Debug.Log("l = " + l);

        float h_min = Mathf.Abs(h_bc - h_de);
        float h_max = h_bc + h_de;

        Debug.Log("h_min = " + h_min + "  h_max = " + h_max);

        float min = Mathf.Sqrt(l * l + h_min * h_min);
        float max = Mathf.Sqrt(l * l + h_max * h_max);

        return new Range<float>(min, max);
    }

    // Calculate h of c
    private float TriangleTall(float a, float b, float c)
    {
        if (CanTriangle(a, b, c))
        {
            float s = (a + b + c) / 2.0f;
            float area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));

            return area * 2.0f / c;
        }

        return 0;
    }

    private bool CanTriangle(float a, float b, float c)
    {
        return (a + b) > c && (a + c) > b && (b + c) > a;
    }


    public string id;

    public void Id()
    {
        Geometry geometry = new TriPyramid();
        Debug.Log(geometry.SignId(id));
    }
}
