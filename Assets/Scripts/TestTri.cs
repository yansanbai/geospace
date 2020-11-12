using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTri : MonoBehaviour
{
    public Vector3 APoint;
    public Vector3 BPoint;
    public Vector3 CPoint;

    public float EdgeALen;
    public float EdgeBLen;
    public float EdgeCLen;
    private Vector3 SPoint;

    public void DrawByEdgeLength()
    {
        SPoint = CalculateTopByEdgeLength();
        SetLines();

        Debug.Log("SA = " + Vector3.Magnitude(SPoint - APoint));
        Debug.Log("SB = " + Vector3.Magnitude(SPoint - BPoint));
        Debug.Log("SC = " + Vector3.Magnitude(SPoint - CPoint));

    }

    public void DrawByRightAngle()
    {
        SPoint = CalculateTopByRightAngle();
        SetLines();

        Debug.Log("SA = " + Vector3.Magnitude(SPoint - APoint));
        Debug.Log("SB = " + Vector3.Magnitude(SPoint - BPoint));
        Debug.Log("SC = " + Vector3.Magnitude(SPoint - CPoint));

    }

    private Vector3 CalculateTopByRightAngle()
    {
        float lenAB = Vector3.Magnitude(APoint - BPoint);
        float lenAC = Vector3.Magnitude(APoint - CPoint);
        float lenBC = Vector3.Magnitude(BPoint - CPoint);

        float powAB = lenAB * lenAB;
        float powAC = lenAC * lenAC;
        float powBC = lenBC * lenBC;

        float lenSA = Mathf.Sqrt((powAB + powAC - powBC) / 2);
        float lenSB = Mathf.Sqrt((powAB + powBC - powAC) / 2);
        float lenSC = Mathf.Sqrt((powBC + powAC - powAB) / 2);
        
        EdgeALen = lenSA;
        EdgeBLen = lenSB;
        EdgeCLen = lenSC;

        return CalculateTopByEdgeLength();
    }

    private Vector3 CalculateTopByEdgeLength()
    {
        Vector3 center;
        Vector3 direction;
        float r;

        bool can = Circle(APoint, BPoint, EdgeALen, EdgeBLen, out center, out direction, out r);
        if (!can)
        {
            Debug.Log("Error 1");
            return Vector3.zero; // No Top
        }

        Vector3 pointD = PointProjectOnPlane(CPoint, center, direction);
        float lenCD = Vector3.Magnitude(pointD - CPoint);
        float d = Vector3.Magnitude(pointD - center);
        float min = Mathf.Sqrt((r - d) * (r - d) + lenCD * lenCD);
        float max = Mathf.Sqrt((r + d) * (r + d) + lenCD * lenCD);

        if (EdgeCLen < min || EdgeCLen > max)
        {
            Debug.Log("Error 2");
            return Vector3.zero; ; // No Top
        }

        float e = Mathf.Sqrt(EdgeCLen * EdgeCLen - lenCD * lenCD);
        float tmp = (e * e - r * r - d * d) / 2 / d;
        float h = Mathf.Sqrt(r * r - tmp * tmp);


        Vector3 dirCenterD = Vector3.Normalize(pointD - center);
        Vector3 dirH = Vector3.Normalize(Vector3.Cross(direction, dirCenterD));

        float d_e = Mathf.Sqrt(e * e - h * h);
        Vector3 pointH = center + dirCenterD * (d - d_e);
        Vector3 pointS = pointH + dirH * h;
        return pointS;

    }

    private bool Circle(Vector3 pointA, Vector3 pointB, float a, float b, out Vector3 center, out Vector3 direction, out float radius)
    {
        direction = Vector3.Normalize(pointA - pointB);
        float c = Vector3.Magnitude(pointA - pointB);

        if (c > (a + b))
        {
            center = Vector3.zero;
            radius = 0;
            return false;
        }

        float tmp = (a * a - b * b - c * c) / 2 / c;
        radius = Mathf.Sqrt(b * b - tmp * tmp);

        float d_a = Mathf.Sqrt(a * a - radius * radius);
        center = pointB + direction * (c - d_a);

        // Debug.Log(" radius: " + radius + "     AB: " + c + "    d_a: " + d_a + " center: " + center);

        return true;
    }

    private Vector3 PointProjectOnPlane(Vector3 point, Vector3 origin, Vector3 normal)
    {
        Vector3 op = point - origin;
        float distance = Vector3.Dot(op, normal);

        return point - distance * normal;
    }


    private void SetLines()
    {
        LineRenderer b_a = GameObject.Find("bottom_a").GetComponent<LineRenderer>();
        LineRenderer b_b = GameObject.Find("bottom_b").GetComponent<LineRenderer>();
        LineRenderer b_c = GameObject.Find("bottom_c").GetComponent<LineRenderer>();

        b_a.SetPosition(0, APoint);
        b_a.SetPosition(1, BPoint);

        b_b.SetPosition(0, APoint);
        b_b.SetPosition(1, CPoint);

        b_c.SetPosition(0, BPoint);
        b_c.SetPosition(1, CPoint);

        LineRenderer e_a = GameObject.Find("edge_a").GetComponent<LineRenderer>();
        LineRenderer e_b = GameObject.Find("edge_b").GetComponent<LineRenderer>();
        LineRenderer e_c = GameObject.Find("edge_c").GetComponent<LineRenderer>();

        e_a.SetPosition(0, APoint);
        e_a.SetPosition(1, SPoint);

        e_b.SetPosition(0, BPoint);
        e_b.SetPosition(1, SPoint);

        e_c.SetPosition(0, CPoint);
        e_c.SetPosition(1, SPoint);
    }


}
