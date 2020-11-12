using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triangular Pyramid
public class TriPyramid : Geometry
{
    public const int BOTTOM_A = 0;
    public const int BOTTOM_B = 1;
    public const int BOTTOM_C = 2;
    public const int TOP_S = 3;
    public override void Init()
    {
        base.Init();

        Name = "Triangular Pyramid";
        Type = GeometryType.TriPyd;

        float sqrt3 = Mathf.Sqrt(3);
        Vector3 faceNormal = Vector3.up;

        VertexFace u0 = new VertexFace(-1, 0, -sqrt3 / 3, faceNormal);
        VertexFace u1 = new VertexFace(1, 0, -sqrt3 / 3, faceNormal);
        VertexFace u2 = new VertexFace(0, 0, sqrt3 * 2 / 3, faceNormal);
        VertexSpace u3 = new VertexSpace(0, sqrt3, 0);
        AddBaseVertex(u0);
        AddBaseVertex(u1);
        AddBaseVertex(u2);
        AddBaseVertex(u3);

        GeoVertex v0 = new GeoVertex(u0, true);
        GeoVertex v1 = new GeoVertex(u1, true);
        GeoVertex v2 = new GeoVertex(u2, true);
        GeoVertex v3 = new GeoVertex(u3, true);
        AddGeoVertex(v0);
        AddGeoVertex(v1);
        AddGeoVertex(v2);
        AddGeoVertex(v3);

        GeoEdge e0 = new GeoEdge(u0, u1, true);
        GeoEdge e1 = new GeoEdge(u0, u2, true);
        GeoEdge e2 = new GeoEdge(u1, u2, true);
        GeoEdge e3 = new GeoEdge(u0, u3, true);
        GeoEdge e4 = new GeoEdge(u1, u3, true);
        GeoEdge e5 = new GeoEdge(u2, u3, true);
        AddGeoEdge(e0);
        AddGeoEdge(e1);
        AddGeoEdge(e2);
        AddGeoEdge(e3);
        AddGeoEdge(e4);
        AddGeoEdge(e5);


        GeoFace f0 = new GeoFace(new VertexUnit[] { u0, u1, u2 }, true);
        GeoFace f1 = new GeoFace(new VertexUnit[] { u1, u0, u3 }, true);
        GeoFace f2 = new GeoFace(new VertexUnit[] { u0, u2, u3 }, true);
        GeoFace f3 = new GeoFace(new VertexUnit[] { u2, u1, u3 }, true);
        AddGeoFace(f0);
        AddGeoFace(f1);
        AddGeoFace(f2);
        AddGeoFace(f3);

        InitDatas();
    }

    public bool IsSignBottom(string sign)
    {
        int index = SignVertex(sign);
        if (index == -1)
            return false;
        if (index == BOTTOM_A || index == BOTTOM_B || index == BOTTOM_C)
            return true;
        return false;
    }

    public bool IsSignTop(string sign)
    {
        int index = SignVertex(sign);
        if (index == -1)
            return false;
        if (index == TOP_S)
            return true;
        return false;
    }

}

public class TriPyramidGeometryTool : GeometryTool
{
    public override Geometry GenerateGeometry()
    {
        TriPyramid triPyramid = new TriPyramid();
        triPyramid.Constructor = new TriPyramidConstructor(triPyramid);
        triPyramid.Assistor = new Assistor(triPyramid);
        triPyramid.Implement = new Implement(triPyramid);
        triPyramid.Init();

        return triPyramid;
    }
}

public class TriPyramidGeometryState : GeometryState
{
    new TriPyramid geometry;

    public TriPyramidGeometryState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is TriPyramid)
            this.geometry = (TriPyramid)geometry;
    }

    public override FormInput Title()
    {
        FormElement formElement1 = new FormElement(1);
        formElement1.fields[0] = geometry.VertexSign(TriPyramid.TOP_S);

        FormElement formElement2 = new FormElement(3);
        formElement2.fields[0] = geometry.VertexSign(TriPyramid.BOTTOM_A);
        formElement2.fields[1] = geometry.VertexSign(TriPyramid.BOTTOM_B);
        formElement2.fields[2] = geometry.VertexSign(TriPyramid.BOTTOM_C);

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement1;
        formInput.inputs[1] = new FormText("-");
        formInput.inputs[2] = formElement2;

        return formInput;
    }
}