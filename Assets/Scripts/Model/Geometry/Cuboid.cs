using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triangular Pyramid
public class Cuboid : Geometry
{
    // Positive +    Negative -

    public const int PNP_A = 0;
    public const int NNP_B = 1;
    public const int NNN_C = 2;
    public const int PNN_D = 3;
    public const int PPP_E = 4;
    public const int NPP_F = 5;
    public const int NPN_G = 6;
    public const int PPN_H = 7;

    private VertexCuboid[] vertexCuboids;

    public override void Init()
    {
        base.Init();

        Name = "Cuboid";
        Type = GeometryType.Cubio;

        VertexCuboid u0 = new VertexCuboid(1, -1, 1);
        VertexCuboid u1 = new VertexCuboid(-1, -1, 1);
        VertexCuboid u2 = new VertexCuboid(-1, -1, -1);
        VertexCuboid u3 = new VertexCuboid(1, -1, -1);
        VertexCuboid u4 = new VertexCuboid(1, 1, 1);
        VertexCuboid u5 = new VertexCuboid(-1, 1, 1);
        VertexCuboid u6 = new VertexCuboid(-1, 1, -1);
        VertexCuboid u7 = new VertexCuboid(1, 1, -1);
        AddBaseVertex(u0);
        AddBaseVertex(u1);
        AddBaseVertex(u2);
        AddBaseVertex(u3);
        AddBaseVertex(u4);
        AddBaseVertex(u5);
        AddBaseVertex(u6);
        AddBaseVertex(u7);

        vertexCuboids = new VertexCuboid[] { u0, u1, u2, u3, u4, u5, u6, u7 };

        GeoVertex v0 = new GeoVertex(u0, true);
        GeoVertex v1 = new GeoVertex(u1, true);
        GeoVertex v2 = new GeoVertex(u2, true);
        GeoVertex v3 = new GeoVertex(u3, true);
        GeoVertex v4 = new GeoVertex(u4, true);
        GeoVertex v5 = new GeoVertex(u5, true);
        GeoVertex v6 = new GeoVertex(u6, true);
        GeoVertex v7 = new GeoVertex(u7, true);
        AddGeoVertex(v0);
        AddGeoVertex(v1);
        AddGeoVertex(v2);
        AddGeoVertex(v3);
        AddGeoVertex(v4);
        AddGeoVertex(v5);
        AddGeoVertex(v6);
        AddGeoVertex(v7);

        GeoEdge e0 = new GeoEdge(u0, u1, true);
        GeoEdge e1 = new GeoEdge(u1, u2, true);
        GeoEdge e2 = new GeoEdge(u2, u3, true);
        GeoEdge e3 = new GeoEdge(u3, u0, true);
        GeoEdge e4 = new GeoEdge(u4, u5, true);
        GeoEdge e5 = new GeoEdge(u5, u6, true);
        GeoEdge e6 = new GeoEdge(u6, u7, true);
        GeoEdge e7 = new GeoEdge(u7, u4, true);
        GeoEdge e8 = new GeoEdge(u0, u4, true);
        GeoEdge e9 = new GeoEdge(u1, u5, true);
        GeoEdge e10 = new GeoEdge(u2, u6, true);
        GeoEdge e11 = new GeoEdge(u3, u7, true);
        AddGeoEdge(e0);
        AddGeoEdge(e1);
        AddGeoEdge(e2);
        AddGeoEdge(e3);
        AddGeoEdge(e4);
        AddGeoEdge(e5);
        AddGeoEdge(e6);
        AddGeoEdge(e7);
        AddGeoEdge(e8);
        AddGeoEdge(e9);
        AddGeoEdge(e10);
        AddGeoEdge(e11);

        GeoFace f0 = new GeoFace(new VertexUnit[] { u0, u1, u2, u3 }, true);
        GeoFace f1 = new GeoFace(new VertexUnit[] { u0, u4, u5, u1 }, true);
        GeoFace f2 = new GeoFace(new VertexUnit[] { u1, u5, u6, u2 }, true);
        GeoFace f3 = new GeoFace(new VertexUnit[] { u2, u6, u7, u3 }, true);
        GeoFace f4 = new GeoFace(new VertexUnit[] { u3, u7, u4, u0 }, true);
        GeoFace f5 = new GeoFace(new VertexUnit[] { u7, u6, u5, u4 }, true);
        AddGeoFace(f0);
        AddGeoFace(f1);
        AddGeoFace(f2);
        AddGeoFace(f3);
        AddGeoFace(f4);
        AddGeoFace(f5);

        InitDatas();
    }

    public override void MoveVertex(VertexUnit vertex, Ray ray, Transform camera, bool snap)
    {
        base.MoveVertex(vertex, ray, camera, snap);
        if (!vertex.isBase)
            return;
        Vector3 position = vertex.Position();
        SetVerticesAbsPosition(position);
    }


    public override VertexUnit[] VerticesOfMoveVertex(VertexUnit vertex)
    {
        if (!vertex.isBase)
            return base.VerticesOfMoveVertex(vertex);
        else
            return (VertexUnit[])vertexCuboids.Clone();
    }

    public void SetVerticesAbsPosition(Vector3 position)
    {
        foreach (VertexCuboid unit in vertexCuboids)
            unit.SetAbsPosition(position);
    }

}

public class CuboidGeometryTool : GeometryTool
{
    public override Geometry GenerateGeometry()
    {
        Cuboid cuboid = new Cuboid();
        cuboid.Constructor = new CuboidConstructor(cuboid);
        cuboid.Assistor = new Assistor(cuboid);
        cuboid.Implement = new Implement(cuboid);
        cuboid.Init();

        return cuboid;
    }
}

public class CuboidGeometryState : GeometryState
{
    new Cuboid geometry;

    public CuboidGeometryState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is Cuboid)
            this.geometry = (Cuboid)geometry;
    }

    public override FormInput Title()
    {
        FormElement formElement = new FormElement(8);
        
        formElement.fields[4] = geometry.VertexSign(Cuboid.PNP_A);
        formElement.fields[5] = geometry.VertexSign(Cuboid.NNP_B);
        formElement.fields[6] = geometry.VertexSign(Cuboid.NNN_C);
        formElement.fields[7] = geometry.VertexSign(Cuboid.PNN_D);
        formElement.fields[0] = geometry.VertexSign(Cuboid.PPP_E);
        formElement.fields[1] = geometry.VertexSign(Cuboid.NPP_F);
        formElement.fields[2] = geometry.VertexSign(Cuboid.NPN_G);
        formElement.fields[3] = geometry.VertexSign(Cuboid.PPN_H);

        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = formElement;

        return formInput;
    }
}