using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpreadAuxiliary : Auxiliary
{
    public FaceRefer face;
    public Circular circular;
    private ResolvedBody resolvedBody;
    private GeoCamera geoCamera;
    private GeometryBehaviour geometryBehaviour;
    private float positionZ;
    public SpreadAuxiliary() : base()
    {
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        geoCamera = GameObject.Find("/3D/GeoCamera").GetComponent<GeoCamera>();
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        units = new VertexUnit[] { };
        if (geometry is ResolvedBody)
            resolvedBody = (ResolvedBody)geometry;
        GeoVertex[] geoVertices = geometry.GeoVertices();
        int[] ids = new int[geoVertices.Length];
        for (int i = 0; i < geoVertices.Length; i++)
        {
            ids[i] = geoVertices[i].VertexUnit().id;
        }
        face = new FaceRefer(ids);

        elements = new GeoElement[] { };

        dependencies.AddRange(units);

        GeoCircular[] geoCirculars = geometry.GeoCirculars();
        if (geoCirculars.Length != 0)
        {
            GeoCircular geoCircular = geoCirculars[0];
            circular = geoCircular.Circular();
        }
    }

    public bool CanSpread() {
        if (circular.IsNormalCircular())
        {
            return true;
        }
        return false;
    }

    public void AddPlaneGraph()
    {
        if (circular.IsNormalCircular())
        {
            if (circular.type == CircularType.Cylinder)
            {
                SpreadCylinder();
                resolvedBody.isSpread = true;
            }
            else if (circular.type == CircularType.Cone)
            {
                SpreadCone();
                resolvedBody.isSpread = true;

            }
        }
    }

    private void SpreadCylinder()
    {
        float Radius = circular.radius1;
        float height = circular.Vertices[0].y - circular.Vertices[1].y;
        float width = 2 * Mathf.PI * Radius;
        positionZ = width / 2;
        geoCamera.TriggerMoveZAnimation(270, 0, positionZ);

        VertexSpace u0 = new VertexSpace(0, height / 2, Radius);
        VertexSpace u1 = new VertexSpace(0, -height / 2, Radius);
        VertexSpace u2 = new VertexSpace(0, -height / 2, Radius + width);
        VertexSpace u3 = new VertexSpace(0, height / 2, Radius + width);

        GeoEdge e0 = new GeoEdge(u0, u1, false);
        GeoEdge e1 = new GeoEdge(u1, u2, false);
        GeoEdge e2 = new GeoEdge(u2, u3, false);
        GeoEdge e3 = new GeoEdge(u0, u3, false);
        geometryBehaviour.AddElement(e0);
        geometryBehaviour.AddElement(e1);
        geometryBehaviour.AddElement(e2);
        geometryBehaviour.AddElement(e3);

        GeoFace f = new GeoFace(new VertexSpace[] { u0, u1, u2, u3 }, false, FaceType.SpreadRectangle);
        geometryBehaviour.AddElement(f);

        VertexSpace u4 = new VertexSpace(0, height / 2 + Radius, width / 2 + Radius);
        GeoCircle c1 = new GeoCircle(u4, Radius, CircleDirection.X, true, FaceType.SpreadCylinderCircle);
        geometryBehaviour.AddElement(c1);
        VertexSpace u5 = new VertexSpace(0, -height / 2 - Radius, width / 2 + Radius);
        GeoCircle c2 = new GeoCircle(u5, Radius, CircleDirection.X, true, FaceType.SpreadCylinderCircle);
        geometryBehaviour.AddElement(c2);
    }

    private void SpreadCone()
    {
        float Radius = circular.radius1;
        float height = circular.Vertices[0].y - circular.Vertices[1].y;
        float width = circular.Vertices[2].z - circular.Vertices[1].z;
        float fanRadius = Mathf.Sqrt(height * height + width * width);
        positionZ = fanRadius / 2;
        geoCamera.TriggerMoveZAnimation(270, 0, positionZ);

        VertexSpace u0 = new VertexSpace(circular.Vertices[0]);
        VertexSpace u1 = new VertexSpace(circular.Vertices[2]);
        VertexSpace u2 = new VertexSpace(circular.Vertices[1]);
        int segments = 60;
        float angleRad = Mathf.Deg2Rad * (360 * Radius / fanRadius);
        float angleCur = Mathf.Deg2Rad * (180 - Mathf.Acos(height / fanRadius) * 180 / Mathf.PI - 360 * Radius / fanRadius);
        float angledelta = angleRad / segments;
        int vertices_count = segments + 1;
        VertexSpace v1;
        VertexSpace v2;
        GeoFace f;
        Vector3[] vertices = new Vector3[vertices_count];
        for (int i = 0; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);
            vertices[i] = new Vector3(u0.Position().x + 0, u0.Position().y + fanRadius * cosA, u0.Position().x + fanRadius * sinA);
            angleCur += angledelta;
        }
        VertexSpace[] vertexs = new VertexSpace[vertices_count + 1];
        vertexs[0] = u0;
        for (int i = 0; i < vertices_count - 1; i++)
        {
            v1 = new VertexSpace(vertices[i]);
            v2 = new VertexSpace(vertices[i + 1]);
            vertexs[i + 1] = v1;
            vertexs[i + 2] = v2;
            geometryBehaviour.AddElement(new GeoEdge(v1, v2, false));
        }
        f = new GeoFace(vertexs, false, FaceType.SpreadFan);
        geometryBehaviour.AddElement(f);
        GeoEdge e1 = new GeoEdge(u0, new VertexSpace(vertices[0]), false);
        GeoEdge e2 = new GeoEdge(u0, new VertexSpace(vertices[vertices_count - 1]), false);
        geometryBehaviour.AddElement(e1);
        geometryBehaviour.AddElement(e2);


        VertexSpace u3 = new VertexSpace(0, circular.Vertices[0].y, fanRadius + Radius);
        GeoCircle c2 = new GeoCircle(u3, Radius, CircleDirection.X, true, FaceType.SpreadConeCircle);
        geometryBehaviour.AddElement(c2);
    }

    public override void RemoveAuxiliary()
    {
        geoCamera.TriggerMoveZAnimation(225, 30, -positionZ);
        geometryBehaviour.clearExtraElements();
        resolvedBody.isSpread = false;
    }
}

public class SpreadAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        return true;
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        if (!(geometry is ResolvedBody))
        {
            return null;
        }

        ResolvedBody resolvedBody = (ResolvedBody)geometry;
        if (!resolvedBody.isSpinned || resolvedBody.isSpread)
        {
            return null;
        }

        SpreadAuxiliary auxiliary = new SpreadAuxiliary();
        auxiliary.InitWithGeometry(geometry);
        if (!auxiliary.CanSpread()) 
        {
            return null;
        }
        auxiliary.AddPlaneGraph();

        return auxiliary;
    }
}

public class SpreadAuxiliaryState : AuxiliaryState
{
    new SpreadAuxiliary auxiliary;
    Geometry geometry;

    public SpreadAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is SpreadAuxiliary)
            this.auxiliary = (SpreadAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        return auxiliary.face.ids;
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = new FormText("展开平面");

        return formInput;
    }
}