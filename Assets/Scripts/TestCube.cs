using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class TestCube : MonoBehaviour
{

    public Vector3 view;
    public Vector3 up;

    public Vector3 vector;
    public void Rotate()
    {
        Quaternion q = Quaternion.LookRotation(view, up);
        Debug.Log(q);
        transform.rotation = q;
        // transform.rotation.SetLookRotation(new Vector3(1, 0, 0), new Vector3(0, 1, 1));
    }

    public void WorldToLocal()
    {
        Vector3 local = transform.InverseTransformDirection(vector);
        Debug.Log(local);
    }

    public Vector2Int edge;
    public int length;

    public Vector3Int corner;
    public int angle;

    private Geometry geometry;

    void Awake()
    {

    }

    public void Init()
    {
        geometry = new TriPyramid();
        geometry.Init();
        geometry.Constructor = new TriPyramidConstructor(geometry);
        Debug.Log("Init!");
    }

    public void AddBottomLength()
    {
        BottomLengthCondition condition = new BottomLengthCondition(edge.x, edge.y, length);
        geometry.Constructor.AddCondition(condition);

        PrintAtttributes();
    }

    public void AddBottomAngle()
    {
        BottomAngleCondition condition = new BottomAngleCondition(corner.x, corner.y, corner.z, angle);
        geometry.Constructor.AddCondition(condition);

        PrintAtttributes();
    }

    public void ClearConditions()
    {
        geometry.Constructor.ClearConditions();
    }

    private void PrintAtttributes()
    {
        Debug.Log("A: " + geometry.UnitVector(0));
        Debug.Log("B: " + geometry.UnitVector(1));
        Debug.Log("C: " + geometry.UnitVector(2));
        Debug.Log("S: " + geometry.UnitVector(3));

        Vector3 ab = geometry.UnitVector(0) - geometry.UnitVector(1);
        Vector3 ac = geometry.UnitVector(0) - geometry.UnitVector(2);
        Vector3 bc = geometry.UnitVector(1) - geometry.UnitVector(2);
        Debug.Log("AB: " + Vector3.Magnitude(ab));
        Debug.Log("AC: " + Vector3.Magnitude(ac));
        Debug.Log("BC: " + Vector3.Magnitude(bc));

        Debug.Log("ABC: " + Vector3.Angle(ab, bc));
        Debug.Log("ACB: " + Vector3.Angle(ac, bc));
        Debug.Log("BAC: " + Vector3.Angle(ab, ac));

    }
}
